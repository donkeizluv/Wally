using System;

namespace Wally.Day_Dream.Favorite
{
    //public interface IFavoriteSaver
    //{
    //    bool Save(PictureData data);
    //    bool Delete(PictureData data);
    //    int FavoriteCount { get; }
    //    event EventHandler<AddedToFavoriteEventAgrs> AddedToFavoriteEvent;
    //    event EventHandler<RemovedFromFavoriteEventAgrs> RemovedFromFavorite;
    //    //IEnumerable<PictureData> GetAll();
    //}
    internal abstract class Favorites
    {
        public abstract int FavoriteCount { get; }
        public abstract bool Save(PictureData data);
        public abstract bool Delete(PictureData data);
        public abstract bool CheckFavorite(PictureData data);
        public static event EventHandler<AddedToFavoriteEventAgrs> AddedToFavoriteEvent;
        public static event EventHandler<RemovedFromFavoriteEventAgrs> RemovedFromFavorite;

        protected virtual void RaiseAddedToFavorite(PictureData data)
            => AddedToFavoriteEvent?.Invoke(this, new AddedToFavoriteEventAgrs(data.PageUrl));

        protected virtual void RaiseRemovedFromFavorite(PictureData data)
            => RemovedFromFavorite?.Invoke(this, new RemovedFromFavoriteEventAgrs(data.PageUrl));
    }
}