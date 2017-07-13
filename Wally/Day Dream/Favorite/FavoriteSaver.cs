using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using LiteDB;
using Wally.Day_Dream.Interfaces;
using Wally.Day_Dream.Scrape;

namespace Wally.Day_Dream.Favorite
{
    public struct FavoritePOCO
    {
        public int Id { get; set; }
        public string ScraperName { get; set; }
        public string ThumbUrl { get; set; }
        public string PageUrl { get; set; }
    }

    internal class FavoritesSaver : Favorites, IPictureDataProvider, IDisposable
    {
        private const string DataFileName = "fav.w";
        private const string CollectionName = "Fav";
        private readonly List<PictureData> _cache = new List<PictureData>();
        private readonly LiteCollection<FavoritePOCO> _collection;
        private readonly string _currentPath;

        private readonly Queue<PictureData> _dataQueue = new Queue<PictureData>();
        private readonly LiteDatabase _db;

        /// <summary>
        ///     path to database's location
        /// </summary>
        /// <summary>
        ///     uses default path (App data)
        /// </summary>
        public FavoritesSaver()
        {
            _currentPath = GetDefaultPath();
            try
            {
                // Get collection
                _db = new LiteDatabase(_currentPath);
                _collection = _db.GetCollection<FavoritePOCO>(CollectionName);
                FillData();
            }
            catch (Exception ex)
            {
                ExManager.Ex(ex);
            }
        }

        public override int FavoriteCount => CountAll();

        public void Dispose()
        {
            _db.Dispose();
        }

        async Task<PictureData> IPictureDataProvider.GetData()
        {
            if (_dataQueue.Count >= 1) return _dataQueue.Dequeue();
            foreach (var d in _cache)
            {
                _dataQueue.Enqueue(d);
            }
            return _dataQueue.Dequeue();
        }

        public bool CanSupplyWhenFail => false;

        private static string GetDefaultPath()
        {
            return Path.Combine(GetDataFolder(), DataFileName);
        }

        private static string GetDataFolder()
        {
            string localDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string fullPath = Path.Combine(localDataPath, GetCompanyName());
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }
            return fullPath;
        }

        private static string GetCompanyName()
        {
            var versionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location);

            return versionInfo.CompanyName;
        }

        //public void Dispose()
        //{
        //    db.Dispose();
        //}

        public override bool Save(PictureData data)
        {
            try
            {
                // Create new entry
                _cache.Add(data);
                var fav = new FavoritePOCO
                {
                    ScraperName = Fussy.EncryptString(data.Scraper.SiteName),
                    ThumbUrl = Fussy.EncryptString(data.ThumbUrl),
                    PageUrl = Fussy.EncryptString(data.PageUrl)
                };
                _collection.Insert(fav);
                _collection.EnsureIndex(x => x.PageUrl);
                RaiseAddedToFavorite(data);
            }
            catch (Exception ex)
            {
                ExManager.Ex(ex);
                return false;
            }
            return true;
        }

        public override bool Delete(PictureData data)
        {
            try
            {
                _cache.RemoveAt(_cache.FindIndex(d => d.PageUrl == data.PageUrl));
                int count = _Delete(data);
                RaiseRemovedFromFavorite(data);
                return count > 0;
            }
            catch (Exception ex)
            {
                ExManager.Ex(ex);
                return false;
            }
        }

        private int _Delete(PictureData data)
        {
            string target = Fussy.EncryptString(data.PageUrl);
            return _collection.Delete(i => i.PageUrl == target);
        }

        private void DeleteById(int id)
        {
            _collection.Delete(i => i.Id == id);
        }

        private void FillData()
        {
            try
            {
                foreach (var data in _collection.FindAll())
                {
                    //remove invalid data
                    if (!CheckValid(data))
                    {
                        DeleteById(data.Id);
                        continue;
                    }
                    _cache.Add(new PictureData(Scraper.GetScraperByName(Fussy.DecryptString(data.ScraperName)))
                    {
                        ThumbUrl = Fussy.DecryptString(data.ThumbUrl),
                        PageUrl = Fussy.DecryptString(data.PageUrl)
                    });
                }
            }
            catch (Exception ex)
            {
                ExManager.Ex(ex);
            }
        }

        private static bool CheckValid(FavoritePOCO data) =>
            !string.IsNullOrEmpty(data.PageUrl) && !string.IsNullOrEmpty(data.ScraperName) && !string.IsNullOrEmpty(data.ThumbUrl);

        private int CountAll()
        {
            return _cache.Count;
        }

        public override bool CheckFavorite(PictureData data)
        {
            try
            {
                bool res = _cache.Exists(d => d.PageUrl == data.PageUrl);
                return res;
            }
            catch (Exception ex)
            {
                ExManager.Ex(ex);
                return false;
            }
        }

        /// should use LiteDB encrytion instead
        private static class Fussy
        {
            // This size of the IV (in bytes) must = (keysize / 8).  Default keysize is 256, so the IV must be
            // 32 bytes long.  Using a 16 character string here gives us 32 bytes when converted to a byte array.
            private const string InitVector = "lolcatzdickbutts";
            private const string pass = "so much doge much wow!";
            // This constant is used to determine the keysize of the encryption algorithm
            private const int Keysize = 256;
            private static readonly byte[] _salt = {1, 2, 3, 3, 2, 1, 76, 76};
            //Encrypt
            public static string EncryptString(string plainText)
            {
                var initVectorBytes = Encoding.UTF8.GetBytes(InitVector);
                var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
                var password = new Rfc2898DeriveBytes(pass, _salt);
                var keyBytes = password.GetBytes(Keysize/8);
                var symmetricKey = new RijndaelManaged();
                symmetricKey.Mode = CipherMode.CBC;
                var encryptor = symmetricKey.CreateEncryptor(keyBytes, initVectorBytes);
                var memoryStream = new MemoryStream();
                var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
                cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                cryptoStream.FlushFinalBlock();
                var cipherTextBytes = memoryStream.ToArray();
                memoryStream.Close();
                cryptoStream.Close();
                return Convert.ToBase64String(cipherTextBytes);
            }

            //Decrypt
            public static string DecryptString(string cipherText)
            {
                var initVectorBytes = Encoding.ASCII.GetBytes(InitVector);
                var cipherTextBytes = Convert.FromBase64String(cipherText);
                var password = new Rfc2898DeriveBytes(pass, _salt);
                var keyBytes = password.GetBytes(Keysize/8);
                var symmetricKey = new RijndaelManaged();
                symmetricKey.Mode = CipherMode.CBC;
                var decryptor = symmetricKey.CreateDecryptor(keyBytes, initVectorBytes);
                var memoryStream = new MemoryStream(cipherTextBytes);
                var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
                var plainTextBytes = new byte[cipherTextBytes.Length];
                int decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                memoryStream.Close();
                cryptoStream.Close();
                return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
            }
        }
    }
}