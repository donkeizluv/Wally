using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Xsl;
using Microsoft.Win32;

namespace Wally.HTML
{
    /// <summary>
    ///     A utility class to get HTML document from HTTP.
    /// </summary>
    internal class HtmlWeb
    {
        /// <summary>
        ///     Represents the method that will handle the PostResponse event.
        /// </summary>
        public delegate void PostResponseHandler(HttpWebRequest request, HttpWebResponse response);

        /// <summary>
        ///     Represents the method that will handle the PreHandleDocument event.
        /// </summary>
        public delegate void PreHandleDocumentHandler(HtmlDocument document);

        /// <summary>
        ///     Represents the method that will handle the PreRequest event.
        /// </summary>
        public delegate bool PreRequestHandler(HttpWebRequest request);

        private static Dictionary<string, string> _mimeTypes;

        private bool _cacheOnly;

        private int _streamBufferSize = 1024;

        private bool _usingCache;

        /// <summary>
        ///     Occurs after an HTTP request has been executed.
        /// </summary>
        public PostResponseHandler PostResponse;

        /// <summary>
        ///     Occurs before an HTML document is handled.
        /// </summary>
        public PreHandleDocumentHandler PreHandleDocument;

        /// <summary>
        ///     Occurs before an HTTP request is executed.
        /// </summary>
        public PreRequestHandler PreRequest;

        /// <summary>
        ///     Gets or Sets a value indicating if document encoding must be automatically detected.
        /// </summary>
        public bool AutoDetectEncoding { get; set; } = true;

        /// <summary>
        ///     Gets or Sets a value indicating whether to get document only from the cache.
        ///     If this is set to true and document is not found in the cache, nothing will be loaded.
        /// </summary>
        public bool CacheOnly
        {
            get { return _cacheOnly; }
            set
            {
                if (value && !UsingCache)
                {
                    throw new HtmlWebException("Cache is not enabled. Set UsingCache to true first.");
                }
                _cacheOnly = value;
            }
        }

        /// <summary>
        ///     Gets or Sets the cache path. If null, no caching mechanism will be used.
        /// </summary>
        public string CachePath { get; set; }

        /// <summary>
        ///     Gets a value indicating if the last document was retrieved from the cache.
        /// </summary>
        public bool FromCache { get; private set; }

        internal static Dictionary<string, string> MimeTypes
        {
            get
            {
                if (_mimeTypes != null)
                {
                    return _mimeTypes;
                }
                _mimeTypes = new Dictionary<string, string>
                {
                    {".3dm", "x-world/x-3dmf"},
                    {".3dmf", "x-world/x-3dmf"},
                    {".a", "application/octet-stream"},
                    {".aab", "application/x-authorware-bin"},
                    {".aam", "application/x-authorware-map"},
                    {".aas", "application/x-authorware-seg"},
                    {".abc", "text/vnd.abc"},
                    {".acgi", "text/html"},
                    {".afl", "video/animaflex"},
                    {".ai", "application/postscript"},
                    {".aif", "audio/aiff"},
                    {".aif", "audio/x-aiff"},
                    {".aifc", "audio/aiff"},
                    {".aifc", "audio/x-aiff"},
                    {".aiff", "audio/aiff"},
                    {".aiff", "audio/x-aiff"},
                    {".aim", "application/x-aim"},
                    {".aip", "text/x-audiosoft-intra"},
                    {".ani", "application/x-navi-animation"},
                    {".aos", "application/x-nokia-9000-communicator-add-on-software"},
                    {".aps", "application/mime"},
                    {".arc", "application/octet-stream"},
                    {".arj", "application/arj"},
                    {".arj", "application/octet-stream"},
                    {".art", "image/x-jg"},
                    {".asf", "video/x-ms-asf"},
                    {".asm", "text/x-asm"},
                    {".asp", "text/asp"},
                    {".asx", "application/x-mplayer2"},
                    {".asx", "video/x-ms-asf"},
                    {".asx", "video/x-ms-asf-plugin"},
                    {".au", "audio/basic"},
                    {".au", "audio/x-au"},
                    {".avi", "application/x-troff-msvideo"},
                    {".avi", "video/avi"},
                    {".avi", "video/msvideo"},
                    {".avi", "video/x-msvideo"},
                    {".avs", "video/avs-video"},
                    {".bcpio", "application/x-bcpio"},
                    {".bin", "application/mac-binary"},
                    {".bin", "application/macbinary"},
                    {".bin", "application/octet-stream"},
                    {".bin", "application/x-binary"},
                    {".bin", "application/x-macbinary"},
                    {".bm", "image/bmp"},
                    {".bmp", "image/bmp"},
                    {".bmp", "image/x-windows-bmp"},
                    {".boo", "application/book"},
                    {".book", "application/book"},
                    {".boz", "application/x-bzip2"},
                    {".bsh", "application/x-bsh"},
                    {".bz", "application/x-bzip"},
                    {".bz2", "application/x-bzip2"},
                    {".c", "text/plain"},
                    {".c", "text/x-c"},
                    {".c++", "text/plain"},
                    {".cat", "application/vnd.ms-pki.seccat"},
                    {".cc", "text/plain"},
                    {".cc", "text/x-c"},
                    {".ccad", "application/clariscad"},
                    {".cco", "application/x-cocoa"},
                    {".cdf", "application/cdf"},
                    {".cdf", "application/x-cdf"},
                    {".cdf", "application/x-netcdf"},
                    {".cer", "application/pkix-cert"},
                    {".cer", "application/x-x509-ca-cert"},
                    {".cha", "application/x-chat"},
                    {".chat", "application/x-chat"},
                    {".class", "application/java"},
                    {".class", "application/java-byte-code"},
                    {".class", "application/x-java-class"},
                    {".com", "application/octet-stream"},
                    {".com", "text/plain"},
                    {".conf", "text/plain"},
                    {".cpio", "application/x-cpio"},
                    {".cpp", "text/x-c"},
                    {".cpt", "application/mac-compactpro"},
                    {".cpt", "application/x-compactpro"},
                    {".cpt", "application/x-cpt"},
                    {".crl", "application/pkcs-crl"},
                    {".crl", "application/pkix-crl"},
                    {".crt", "application/pkix-cert"},
                    {".crt", "application/x-x509-ca-cert"},
                    {".crt", "application/x-x509-user-cert"},
                    {".csh", "application/x-csh"},
                    {".csh", "text/x-script.csh"},
                    {".css", "application/x-pointplus"},
                    {".css", "text/css"},
                    {".cxx", "text/plain"},
                    {".dcr", "application/x-director"},
                    {".deepv", "application/x-deepv"},
                    {".def", "text/plain"},
                    {".der", "application/x-x509-ca-cert"},
                    {".dif", "video/x-dv"},
                    {".dir", "application/x-director"},
                    {".dl", "video/dl"},
                    {".dl", "video/x-dl"},
                    {".doc", "application/msword"},
                    {".dot", "application/msword"},
                    {".dp", "application/commonground"},
                    {".drw", "application/drafting"},
                    {".dump", "application/octet-stream"},
                    {".dv", "video/x-dv"},
                    {".dvi", "application/x-dvi"},
                    {".dwf", "model/vnd.dwf"},
                    {".dwg", "application/acad"},
                    {".dwg", "image/vnd.dwg"},
                    {".dwg", "image/x-dwg"},
                    {".dxf", "application/dxf"},
                    {".dxf", "image/vnd.dwg"},
                    {".dxf", "image/x-dwg"},
                    {".dxr", "application/x-director"},
                    {".el", "text/x-script.elisp"},
                    {".elc", "application/x-bytecode.elisp"},
                    {".elc", "application/x-elc"},
                    {".env", "application/x-envoy"},
                    {".eps", "application/postscript"},
                    {".es", "application/x-esrehber"},
                    {".etx", "text/x-setext"},
                    {".evy", "application/envoy"},
                    {".evy", "application/x-envoy"},
                    {".exe", "application/octet-stream"},
                    {".f", "text/plain"},
                    {".f", "text/x-fortran"},
                    {".f77", "text/x-fortran"},
                    {".f90", "text/plain"},
                    {".f90", "text/x-fortran"},
                    {".fdf", "application/vnd.fdf"},
                    {".fif", "application/fractals"},
                    {".fif", "image/fif"},
                    {".fli", "video/fli"},
                    {".fli", "video/x-fli"},
                    {".flo", "image/florian"},
                    {".flx", "text/vnd.fmi.flexstor"},
                    {".fmf", "video/x-atomic3d-feature"},
                    {".for", "text/plain"},
                    {".for", "text/x-fortran"},
                    {".fpx", "image/vnd.fpx"},
                    {".fpx", "image/vnd.net-fpx"},
                    {".frl", "application/freeloader"},
                    {".funk", "audio/make"},
                    {".g", "text/plain"},
                    {".g3", "image/g3fax"},
                    {".gif", "image/gif"},
                    {".gl", "video/gl"},
                    {".gl", "video/x-gl"},
                    {".gsd", "audio/x-gsm"},
                    {".gsm", "audio/x-gsm"},
                    {".gsp", "application/x-gsp"},
                    {".gss", "application/x-gss"},
                    {".gtar", "application/x-gtar"},
                    {".gz", "application/x-compressed"},
                    {".gz", "application/x-gzip"},
                    {".gzip", "application/x-gzip"},
                    {".gzip", "multipart/x-gzip"},
                    {".h", "text/plain"},
                    {".h", "text/x-h"},
                    {".hdf", "application/x-hdf"},
                    {".help", "application/x-helpfile"},
                    {".hgl", "application/vnd.hp-hpgl"},
                    {".hh", "text/plain"},
                    {".hh", "text/x-h"},
                    {".hlb", "text/x-script"},
                    {".hlp", "application/hlp"},
                    {".hlp", "application/x-helpfile"},
                    {".hlp", "application/x-winhelp"},
                    {".hpg", "application/vnd.hp-hpgl"},
                    {".hpgl", "application/vnd.hp-hpgl"},
                    {".hqx", "application/binhex"},
                    {".hqx", "application/binhex4"},
                    {".hqx", "application/mac-binhex"},
                    {".hqx", "application/mac-binhex40"},
                    {".hqx", "application/x-binhex40"},
                    {".hqx", "application/x-mac-binhex40"},
                    {".hta", "application/hta"},
                    {".htc", "text/x-component"},
                    {".htm", "text/html"},
                    {".html", "text/html"},
                    {".htmls", "text/html"},
                    {".htt", "text/webviewhtml"},
                    {".htx", "text/html"},
                    {".ice", "x-conference/x-cooltalk"},
                    {".ico", "image/x-icon"},
                    {".idc", "text/plain"},
                    {".ief", "image/ief"},
                    {".iefs", "image/ief"},
                    {".iges", "application/iges"},
                    {".iges", "model/iges"},
                    {".igs", "application/iges"},
                    {".igs", "model/iges"},
                    {".ima", "application/x-ima"},
                    {".imap", "application/x-httpd-imap"},
                    {".inf", "application/inf"},
                    {".ins", "application/x-internett-signup"},
                    {".ip", "application/x-ip2"},
                    {".isu", "video/x-isvideo"},
                    {".it", "audio/it"},
                    {".iv", "application/x-inventor"},
                    {".ivr", "i-world/i-vrml"},
                    {".ivy", "application/x-livescreen"},
                    {".jam", "audio/x-jam"},
                    {".jav", "text/plain"},
                    {".jav", "text/x-java-source"},
                    {".java", "text/plain"},
                    {".java", "text/x-java-source"},
                    {".jcm", "application/x-java-commerce"},
                    {".jfif", "image/jpeg"},
                    {".jfif", "image/pjpeg"},
                    {".jfif-tbnl", "image/jpeg"},
                    {".jpe", "image/jpeg"},
                    {".jpe", "image/pjpeg"},
                    {".jpeg", "image/jpeg"},
                    {".jpeg", "image/pjpeg"},
                    {".jpg", "image/jpeg"},
                    {".jpg", "image/pjpeg"},
                    {".jps", "image/x-jps"},
                    {".js", "application/x-javascript"},
                    {".js", "application/javascript"},
                    {".js", "application/ecmascript"},
                    {".js", "text/javascript"},
                    {".js", "text/ecmascript"},
                    {".jut", "image/jutvision"},
                    {".kar", "audio/midi"},
                    {".kar", "music/x-karaoke"},
                    {".ksh", "application/x-ksh"},
                    {".ksh", "text/x-script.ksh"},
                    {".la", "audio/nspaudio"},
                    {".la", "audio/x-nspaudio"},
                    {".lam", "audio/x-liveaudio"},
                    {".latex", "application/x-latex"},
                    {".lha", "application/lha"},
                    {".lha", "application/octet-stream"},
                    {".lha", "application/x-lha"},
                    {".lhx", "application/octet-stream"},
                    {".list", "text/plain"},
                    {".lma", "audio/nspaudio"},
                    {".lma", "audio/x-nspaudio"},
                    {".log", "text/plain"},
                    {".lsp", "application/x-lisp"},
                    {".lsp", "text/x-script.lisp"},
                    {".lst", "text/plain"},
                    {".lsx", "text/x-la-asf"},
                    {".ltx", "application/x-latex"},
                    {".lzh", "application/octet-stream"},
                    {".lzh", "application/x-lzh"},
                    {".lzx", "application/lzx"},
                    {".lzx", "application/octet-stream"},
                    {".lzx", "application/x-lzx"},
                    {".m", "text/plain"},
                    {".m", "text/x-m"},
                    {".m1v", "video/mpeg"},
                    {".m2a", "audio/mpeg"},
                    {".m2v", "video/mpeg"},
                    {".m3u", "audio/x-mpequrl"},
                    {".man", "application/x-troff-man"},
                    {".map", "application/x-navimap"},
                    {".mar", "text/plain"},
                    {".mbd", "application/mbedlet"},
                    {".mc$", "application/x-magic-cap-package-1.0"},
                    {".mcd", "application/mcad"},
                    {".mcd", "application/x-mathcad"},
                    {".mcf", "image/vasa"},
                    {".mcf", "text/mcf"},
                    {".mcp", "application/netmc"},
                    {".me", "application/x-troff-me"},
                    {".mht", "message/rfc822"},
                    {".mhtml", "message/rfc822"},
                    {".mid", "application/x-midi"},
                    {".mid", "audio/midi"},
                    {".mid", "audio/x-mid"},
                    {".mid", "audio/x-midi"},
                    {".mid", "music/crescendo"},
                    {".mid", "x-music/x-midi"},
                    {".midi", "application/x-midi"},
                    {".midi", "audio/midi"},
                    {".midi", "audio/x-mid"},
                    {".midi", "audio/x-midi"},
                    {".midi", "music/crescendo"},
                    {".midi", "x-music/x-midi"},
                    {".mif", "application/x-frame"},
                    {".mif", "application/x-mif"},
                    {".mime", "message/rfc822"},
                    {".mime", "www/mime"},
                    {".mjf", "audio/x-vnd.audioexplosion.mjuicemediafile"},
                    {".mjpg", "video/x-motion-jpeg"},
                    {".mm", "application/base64"},
                    {".mm", "application/x-meme"},
                    {".mme", "application/base64"},
                    {".mod", "audio/mod"},
                    {".mod", "audio/x-mod"},
                    {".moov", "video/quicktime"},
                    {".mov", "video/quicktime"},
                    {".movie", "video/x-sgi-movie"},
                    {".mp2", "audio/mpeg"},
                    {".mp2", "audio/x-mpeg"},
                    {".mp2", "video/mpeg"},
                    {".mp2", "video/x-mpeg"},
                    {".mp2", "video/x-mpeq2a"},
                    {".mp3", "audio/mpeg3"},
                    {".mp3", "audio/x-mpeg-3"},
                    {".mp3", "video/mpeg"},
                    {".mp3", "video/x-mpeg"},
                    {".mpa", "audio/mpeg"},
                    {".mpa", "video/mpeg"},
                    {".mpc", "application/x-project"},
                    {".mpe", "video/mpeg"},
                    {".mpeg", "video/mpeg"},
                    {".mpg", "audio/mpeg"},
                    {".mpg", "video/mpeg"},
                    {".mpga", "audio/mpeg"},
                    {".mpp", "application/vnd.ms-project"},
                    {".mpt", "application/x-project"},
                    {".mpv", "application/x-project"},
                    {".mpx", "application/x-project"},
                    {".mrc", "application/marc"},
                    {".ms", "application/x-troff-ms"},
                    {".mv", "video/x-sgi-movie"},
                    {".my", "audio/make"},
                    {".mzz", "application/x-vnd.audioexplosion.mzz"},
                    {".nap", "image/naplps"},
                    {".naplps", "image/naplps"},
                    {".nc", "application/x-netcdf"},
                    {".ncm", "application/vnd.nokia.configuration-message"},
                    {".nif", "image/x-niff"},
                    {".niff", "image/x-niff"},
                    {".nix", "application/x-mix-transfer"},
                    {".nsc", "application/x-conference"},
                    {".nvd", "application/x-navidoc"},
                    {".o", "application/octet-stream"},
                    {".oda", "application/oda"},
                    {".omc", "application/x-omc"},
                    {".omcd", "application/x-omcdatamaker"},
                    {".omcr", "application/x-omcregerator"},
                    {".p", "text/x-pascal"},
                    {".p10", "application/pkcs10"},
                    {".p10", "application/x-pkcs10"},
                    {".p12", "application/pkcs-12"},
                    {".p12", "application/x-pkcs12"},
                    {".p7a", "application/x-pkcs7-signature"},
                    {".p7c", "application/pkcs7-mime"},
                    {".p7c", "application/x-pkcs7-mime"},
                    {".p7m", "application/pkcs7-mime"},
                    {".p7m", "application/x-pkcs7-mime"},
                    {".p7r", "application/x-pkcs7-certreqresp"},
                    {".p7s", "application/pkcs7-signature"},
                    {".part", "application/pro_eng"},
                    {".pas", "text/pascal"},
                    {".pbm", "image/x-portable-bitmap"},
                    {".pcl", "application/vnd.hp-pcl"},
                    {".pcl", "application/x-pcl"},
                    {".pct", "image/x-pict"},
                    {".pcx", "image/x-pcx"},
                    {".pdb", "chemical/x-pdb"},
                    {".pdf", "application/pdf"},
                    {".pfunk", "audio/make"},
                    {".pfunk", "audio/make.my.funk"},
                    {".pgm", "image/x-portable-graymap"},
                    {".pgm", "image/x-portable-greymap"},
                    {".pic", "image/pict"},
                    {".pict", "image/pict"},
                    {".pkg", "application/x-newton-compatible-pkg"},
                    {".pko", "application/vnd.ms-pki.pko"},
                    {".pl", "text/plain"},
                    {".pl", "text/x-script.perl"},
                    {".plx", "application/x-pixclscript"},
                    {".pm", "image/x-xpixmap"},
                    {".pm", "text/x-script.perl-module"},
                    {".pm4", "application/x-pagemaker"},
                    {".pm5", "application/x-pagemaker"},
                    {".png", "image/png"},
                    {".pnm", "application/x-portable-anymap"},
                    {".pnm", "image/x-portable-anymap"},
                    {".pot", "application/mspowerpoint"},
                    {".pot", "application/vnd.ms-powerpoint"},
                    {".pov", "model/x-pov"},
                    {".ppa", "application/vnd.ms-powerpoint"},
                    {".ppm", "image/x-portable-pixmap"},
                    {".pps", "application/mspowerpoint"},
                    {".pps", "application/vnd.ms-powerpoint"},
                    {".ppt", "application/mspowerpoint"},
                    {".ppt", "application/powerpoint"},
                    {".ppt", "application/vnd.ms-powerpoint"},
                    {".ppt", "application/x-mspowerpoint"},
                    {".ppz", "application/mspowerpoint"},
                    {".pre", "application/x-freelance"},
                    {".prt", "application/pro_eng"},
                    {".ps", "application/postscript"},
                    {".psd", "application/octet-stream"},
                    {".pvu", "paleovu/x-pv"},
                    {".pwz", "application/vnd.ms-powerpoint"},
                    {".py", "text/x-script.phyton"},
                    {".pyc", "applicaiton/x-bytecode.python"},
                    {".qcp", "audio/vnd.qcelp"},
                    {".qd3", "x-world/x-3dmf"},
                    {".qd3d", "x-world/x-3dmf"},
                    {".qif", "image/x-quicktime"},
                    {".qt", "video/quicktime"},
                    {".qtc", "video/x-qtc"},
                    {".qti", "image/x-quicktime"},
                    {".qtif", "image/x-quicktime"},
                    {".ra", "audio/x-pn-realaudio"},
                    {".ra", "audio/x-pn-realaudio-plugin"},
                    {".ra", "audio/x-realaudio"},
                    {".ram", "audio/x-pn-realaudio"},
                    {".ras", "application/x-cmu-raster"},
                    {".ras", "image/cmu-raster"},
                    {".ras", "image/x-cmu-raster"},
                    {".rast", "image/cmu-raster"},
                    {".rexx", "text/x-script.rexx"},
                    {".rf", "image/vnd.rn-realflash"},
                    {".rgb", "image/x-rgb"},
                    {".rm", "application/vnd.rn-realmedia"},
                    {".rm", "audio/x-pn-realaudio"},
                    {".rmi", "audio/mid"},
                    {".rmm", "audio/x-pn-realaudio"},
                    {".rmp", "audio/x-pn-realaudio"},
                    {".rmp", "audio/x-pn-realaudio-plugin"},
                    {".rng", "application/ringing-tones"},
                    {".rng", "application/vnd.nokia.ringing-tone"},
                    {".rnx", "application/vnd.rn-realplayer"},
                    {".roff", "application/x-troff"},
                    {".rp", "image/vnd.rn-realpix"},
                    {".rpm", "audio/x-pn-realaudio-plugin"},
                    {".rt", "text/richtext"},
                    {".rt", "text/vnd.rn-realtext"},
                    {".rtf", "application/rtf"},
                    {".rtf", "application/x-rtf"},
                    {".rtf", "text/richtext"},
                    {".rtx", "application/rtf"},
                    {".rtx", "text/richtext"},
                    {".rv", "video/vnd.rn-realvideo"},
                    {".s", "text/x-asm"},
                    {".s3m", "audio/s3m"},
                    {".saveme", "application/octet-stream"},
                    {".sbk", "application/x-tbook"},
                    {".scm", "application/x-lotusscreencam"},
                    {".scm", "text/x-script.guile"},
                    {".scm", "text/x-script.scheme"},
                    {".scm", "video/x-scm"},
                    {".sdml", "text/plain"},
                    {".sdp", "application/sdp"},
                    {".sdp", "application/x-sdp"},
                    {".sdr", "application/sounder"},
                    {".sea", "application/sea"},
                    {".sea", "application/x-sea"},
                    {".set", "application/set"},
                    {".sgm", "text/sgml"},
                    {".sgm", "text/x-sgml"},
                    {".sgml", "text/sgml"},
                    {".sgml", "text/x-sgml"},
                    {".sh", "application/x-bsh"},
                    {".sh", "application/x-sh"},
                    {".sh", "application/x-shar"},
                    {".sh", "text/x-script.sh"},
                    {".shar", "application/x-bsh"},
                    {".shar", "application/x-shar"},
                    {".shtml", "text/html"},
                    {".shtml", "text/x-server-parsed-html"},
                    {".sid", "audio/x-psid"},
                    {".sit", "application/x-sit"},
                    {".sit", "application/x-stuffit"},
                    {".skd", "application/x-koan"},
                    {".skm", "application/x-koan"},
                    {".skp", "application/x-koan"},
                    {".skt", "application/x-koan"},
                    {".sl", "application/x-seelogo"},
                    {".smi", "application/smil"},
                    {".smil", "application/smil"},
                    {".snd", "audio/basic"},
                    {".snd", "audio/x-adpcm"},
                    {".sol", "application/solids"},
                    {".spc", "application/x-pkcs7-certificates"},
                    {".spc", "text/x-speech"},
                    {".spl", "application/futuresplash"},
                    {".spr", "application/x-sprite"},
                    {".sprite", "application/x-sprite"},
                    {".src", "application/x-wais-source"},
                    {".ssi", "text/x-server-parsed-html"},
                    {".ssm", "application/streamingmedia"},
                    {".sst", "application/vnd.ms-pki.certstore"},
                    {".step", "application/step"},
                    {".stl", "application/sla"},
                    {".stl", "application/vnd.ms-pki.stl"},
                    {".stl", "application/x-navistyle"},
                    {".stp", "application/step"},
                    {".sv4cpio", "application/x-sv4cpio"},
                    {".sv4crc", "application/x-sv4crc"},
                    {".svf", "image/vnd.dwg"},
                    {".svf", "image/x-dwg"},
                    {".svr", "application/x-world"},
                    {".svr", "x-world/x-svr"},
                    {".swf", "application/x-shockwave-flash"},
                    {".t", "application/x-troff"},
                    {".talk", "text/x-speech"},
                    {".tar", "application/x-tar"},
                    {".tbk", "application/toolbook"},
                    {".tbk", "application/x-tbook"},
                    {".tcl", "application/x-tcl"},
                    {".tcl", "text/x-script.tcl"},
                    {".tcsh", "text/x-script.tcsh"},
                    {".tex", "application/x-tex"},
                    {".texi", "application/x-texinfo"},
                    {".texinfo", "application/x-texinfo"},
                    {".text", "application/plain"},
                    {".text", "text/plain"},
                    {".tgz", "application/gnutar"},
                    {".tgz", "application/x-compressed"},
                    {".tif", "image/tiff"},
                    {".tif", "image/x-tiff"},
                    {".tiff", "image/tiff"},
                    {".tiff", "image/x-tiff"},
                    {".tr", "application/x-troff"},
                    {".tsi", "audio/tsp-audio"},
                    {".tsp", "application/dsptype"},
                    {".tsp", "audio/tsplayer"},
                    {".tsv", "text/tab-separated-values"},
                    {".turbot", "image/florian"},
                    {".txt", "text/plain"},
                    {".uil", "text/x-uil"},
                    {".uni", "text/uri-list"},
                    {".unis", "text/uri-list"},
                    {".unv", "application/i-deas"},
                    {".uri", "text/uri-list"},
                    {".uris", "text/uri-list"},
                    {".ustar", "application/x-ustar"},
                    {".ustar", "multipart/x-ustar"},
                    {".uu", "application/octet-stream"},
                    {".uu", "text/x-uuencode"},
                    {".uue", "text/x-uuencode"},
                    {".vcd", "application/x-cdlink"},
                    {".vcs", "text/x-vcalendar"},
                    {".vda", "application/vda"},
                    {".vdo", "video/vdo"},
                    {".vew", "application/groupwise"},
                    {".viv", "video/vivo"},
                    {".viv", "video/vnd.vivo"},
                    {".vivo", "video/vivo"},
                    {".vivo", "video/vnd.vivo"},
                    {".vmd", "application/vocaltec-media-desc"},
                    {".vmf", "application/vocaltec-media-file"},
                    {".voc", "audio/voc"},
                    {".voc", "audio/x-voc"},
                    {".vos", "video/vosaic"},
                    {".vox", "audio/voxware"},
                    {".vqe", "audio/x-twinvq-plugin"},
                    {".vqf", "audio/x-twinvq"},
                    {".vql", "audio/x-twinvq-plugin"},
                    {".vrml", "application/x-vrml"},
                    {".vrml", "model/vrml"},
                    {".vrml", "x-world/x-vrml"},
                    {".vrt", "x-world/x-vrt"},
                    {".vsd", "application/x-visio"},
                    {".vst", "application/x-visio"},
                    {".vsw", "application/x-visio"},
                    {".w60", "application/wordperfect6.0"},
                    {".w61", "application/wordperfect6.1"},
                    {".w6w", "application/msword"},
                    {".wav", "audio/wav"},
                    {".wav", "audio/x-wav"},
                    {".wb1", "application/x-qpro"},
                    {".wbmp", "image/vnd.wap.wbmp"},
                    {".web", "application/vnd.xara"},
                    {".wiz", "application/msword"},
                    {".wk1", "application/x-123"},
                    {".wmf", "windows/metafile"},
                    {".wml", "text/vnd.wap.wml"},
                    {".wmlc", "application/vnd.wap.wmlc"},
                    {".wmls", "text/vnd.wap.wmlscript"},
                    {".wmlsc", "application/vnd.wap.wmlscriptc"},
                    {".word", "application/msword"},
                    {".wp", "application/wordperfect"},
                    {".wp5", "application/wordperfect"},
                    {".wp5", "application/wordperfect6.0"},
                    {".wp6", "application/wordperfect"},
                    {".wpd", "application/wordperfect"},
                    {".wpd", "application/x-wpwin"},
                    {".wq1", "application/x-lotus"},
                    {".wri", "application/mswrite"},
                    {".wri", "application/x-wri"},
                    {".wrl", "application/x-world"},
                    {".wrl", "model/vrml"},
                    {".wrl", "x-world/x-vrml"},
                    {".wrz", "model/vrml"},
                    {".wrz", "x-world/x-vrml"},
                    {".wsc", "text/scriplet"},
                    {".wsrc", "application/x-wais-source"},
                    {".wtk", "application/x-wintalk"},
                    {".xbm", "image/x-xbitmap"},
                    {".xbm", "image/x-xbm"},
                    {".xbm", "image/xbm"},
                    {".xdr", "video/x-amt-demorun"},
                    {".xgz", "xgl/drawing"},
                    {".xif", "image/vnd.xiff"},
                    {".xl", "application/excel"},
                    {".xla", "application/excel"},
                    {".xla", "application/x-excel"},
                    {".xla", "application/x-msexcel"},
                    {".xlb", "application/excel"},
                    {".xlb", "application/vnd.ms-excel"},
                    {".xlb", "application/x-excel"},
                    {".xlc", "application/excel"},
                    {".xlc", "application/vnd.ms-excel"},
                    {".xlc", "application/x-excel"},
                    {".xld", "application/excel"},
                    {".xld", "application/x-excel"},
                    {".xlk", "application/excel"},
                    {".xlk", "application/x-excel"},
                    {".xll", "application/excel"},
                    {".xll", "application/vnd.ms-excel"},
                    {".xll", "application/x-excel"},
                    {".xlm", "application/excel"},
                    {".xlm", "application/vnd.ms-excel"},
                    {".xlm", "application/x-excel"},
                    {".xls", "application/excel"},
                    {".xls", "application/vnd.ms-excel"},
                    {".xls", "application/x-excel"},
                    {".xls", "application/x-msexcel"},
                    {".xlt", "application/excel"},
                    {".xlt", "application/x-excel"},
                    {".xlv", "application/excel"},
                    {".xlv", "application/x-excel"},
                    {".xlw", "application/excel"},
                    {".xlw", "application/vnd.ms-excel"},
                    {".xlw", "application/x-excel"},
                    {".xlw", "application/x-msexcel"},
                    {".xm", "audio/xm"},
                    {".xml", "application/xml"},
                    {".xml", "text/xml"},
                    {".xmz", "xgl/movie"},
                    {".xpix", "application/x-vnd.ls-xpix"},
                    {".xpm", "image/x-xpixmap"},
                    {".xpm", "image/xpm"},
                    {".x-png", "image/png"},
                    {".xsr", "video/x-amt-showrun"},
                    {".xwd", "image/x-xwd"},
                    {".xwd", "image/x-xwindowdump"},
                    {".xyz", "chemical/x-pdb"},
                    {".z", "application/x-compress"},
                    {".z", "application/x-compressed"},
                    {".zip", "application/x-compressed"},
                    {".zip", "application/x-zip-compressed"},
                    {".zip", "application/zip"},
                    {".zip", "multipart/x-zip"},
                    {".zoo", "application/octet-stream"},
                    {".zsh", "text/x-script.zsh"}
                };
                return _mimeTypes;
            }
        }

        /// <summary>
        ///     Gets or sets the Encoding used to override the response stream from any web request
        /// </summary>
        public Encoding OverrideEncoding { get; set; }

        /// <summary>
        ///     Gets the last request duration in milliseconds.
        /// </summary>
        public int RequestDuration { get; private set; }

        /// <summary>
        ///     Gets the URI of the Internet resource that actually responded to the request.
        /// </summary>
        public Uri ResponseUri { get; private set; }

        /// <summary>
        ///     Gets the last request status.
        /// </summary>
        public HttpStatusCode StatusCode { get; private set; } = HttpStatusCode.OK;

        /// <summary>
        ///     Gets or Sets the size of the buffer used for memory operations.
        /// </summary>
        public int StreamBufferSize
        {
            get { return _streamBufferSize; }
            set
            {
                if (_streamBufferSize <= 0)
                {
                    throw new ArgumentException("Size must be greater than zero.");
                }
                _streamBufferSize = value;
            }
        }

        /// <summary>
        ///     Gets or Sets a value indicating if cookies will be stored.
        /// </summary>
        public bool UseCookies { get; set; }

        /// <summary>
        ///     Gets or Sets the User Agent HTTP 1.1 header sent on any webrequest
        /// </summary>
        public string UserAgent { get; set; } =
            "Mozilla/5.0 (Windows; U; Windows NT 5.1; en-US; rv:x.x.x) Gecko/20041107 Firefox/x.x";

        /// <summary>
        ///     Gets or Sets a value indicating whether the caching mechanisms should be used or not.
        /// </summary>
        public bool UsingCache
        {
            get
            {
                if (CachePath == null)
                {
                    return false;
                }
                return _usingCache;
            }
            set
            {
                if (value && CachePath == null)
                {
                    throw new HtmlWebException("You need to define a CachePath first.");
                }
                _usingCache = value;
            }
        }

        /// <summary>
        ///     Creates an instance of the given type from the specified Internet resource.
        /// </summary>
        /// <param name="htmlUrl">The requested URL, such as "http://Myserver/Mypath/Myfile.asp".</param>
        /// <param name="xsltUrl">The URL that specifies the XSLT stylesheet to load.</param>
        /// <param name="xsltArgs">
        ///     An <see cref="T:System.Xml.Xsl.XsltArgumentList" /> containing the namespace-qualified arguments
        ///     used as input to the transform.
        /// </param>
        /// <param name="type">The requested type.</param>
        /// <returns>An newly created instance.</returns>
        public object CreateInstance(string htmlUrl, string xsltUrl, XsltArgumentList xsltArgs, Type type)
        {
            return CreateInstance(htmlUrl, xsltUrl, xsltArgs, type, null);
        }

        /// <summary>
        ///     Creates an instance of the given type from the specified Internet resource.
        /// </summary>
        /// <param name="htmlUrl">The requested URL, such as "http://Myserver/Mypath/Myfile.asp".</param>
        /// <param name="xsltUrl">The URL that specifies the XSLT stylesheet to load.</param>
        /// <param name="xsltArgs">
        ///     An <see cref="T:System.Xml.Xsl.XsltArgumentList" /> containing the namespace-qualified arguments
        ///     used as input to the transform.
        /// </param>
        /// <param name="type">The requested type.</param>
        /// <param name="xmlPath">
        ///     A file path where the temporary XML before transformation will be saved. Mostly used for
        ///     debugging purposes.
        /// </param>
        /// <returns>An newly created instance.</returns>
        public object CreateInstance(string htmlUrl, string xsltUrl, XsltArgumentList xsltArgs, Type type,
            string xmlPath)
        {
            object o;
            var sw = new StringWriter();
            var writer = new XmlTextWriter(sw);
            if (xsltUrl == null)
            {
                LoadHtmlAsXml(htmlUrl, writer);
            }
            else if (xmlPath != null)
            {
                LoadHtmlAsXml(htmlUrl, xsltUrl, xsltArgs, writer, xmlPath);
            }
            else
            {
                LoadHtmlAsXml(htmlUrl, xsltUrl, xsltArgs, writer);
            }
            writer.Flush();
            var reader = new XmlTextReader(new StringReader(sw.ToString()));
            var serializer = new XmlSerializer(type);
            try
            {
                o = serializer.Deserialize(reader);
            }
            catch (InvalidOperationException invalidOperationException)
            {
                throw new Exception(string.Concat(invalidOperationException, ", --- xml:", sw));
            }
            return o;
        }

        /// <summary>
        ///     Creates an instance of the given type from the specified Internet resource.
        /// </summary>
        /// <param name="url">The requested URL, such as "http://Myserver/Mypath/Myfile.asp".</param>
        /// <param name="type">The requested type.</param>
        /// <returns>An newly created instance.</returns>
        public object CreateInstance(string url, Type type)
        {
            return CreateInstance(url, null, null, type);
        }

        private static void FilePreparePath(string target)
        {
            if (File.Exists(target))
            {
                File.SetAttributes(target,
                    File.GetAttributes(target) &
                    (FileAttributes.Hidden | FileAttributes.System | FileAttributes.Directory | FileAttributes.Archive |
                     FileAttributes.Device | FileAttributes.Normal | FileAttributes.Temporary |
                     FileAttributes.SparseFile | FileAttributes.ReparsePoint | FileAttributes.Compressed |
                     FileAttributes.Offline | FileAttributes.NotContentIndexed | FileAttributes.Encrypted |
                     FileAttributes.IntegrityStream | FileAttributes.NoScrubData));
                return;
            }
            string dir = Path.GetDirectoryName(target);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }

        /// <summary>
        ///     Gets an HTML document from an Internet resource and saves it to the specified file.
        /// </summary>
        /// <param name="url">The requested URL, such as "http://Myserver/Mypath/Myfile.asp".</param>
        /// <param name="path">The location of the file where you want to save the document.</param>
        public void Get(string url, string path)
        {
            Get(url, path, "GET");
        }

        /// <summary>
        ///     Gets an HTML document from an Internet resource and saves it to the specified file. - Proxy aware
        /// </summary>
        /// <param name="url">The requested URL, such as "http://Myserver/Mypath/Myfile.asp".</param>
        /// <param name="path">The location of the file where you want to save the document.</param>
        /// <param name="proxy"></param>
        /// <param name="credentials"></param>
        public void Get(string url, string path, WebProxy proxy, NetworkCredential credentials)
        {
            Get(url, path, proxy, credentials, "GET");
        }

        /// <summary>
        ///     Gets an HTML document from an Internet resource and saves it to the specified file.
        /// </summary>
        /// <param name="url">The requested URL, such as "http://Myserver/Mypath/Myfile.asp".</param>
        /// <param name="path">The location of the file where you want to save the document.</param>
        /// <param name="method">The HTTP method used to open the connection, such as GET, POST, PUT, or PROPFIND.</param>
        public void Get(string url, string path, string method)
        {
            var uri = new Uri(url);
            if (!(uri.Scheme == Uri.UriSchemeHttps) && !(uri.Scheme == Uri.UriSchemeHttp))
            {
                throw new HtmlWebException(string.Concat("Unsupported uri scheme: '", uri.Scheme, "'."));
            }
            Get(uri, method, path, null, null, null);
        }

        /// <summary>
        ///     Gets an HTML document from an Internet resource and saves it to the specified file.  Understands Proxies
        /// </summary>
        /// <param name="url">The requested URL, such as "http://Myserver/Mypath/Myfile.asp".</param>
        /// <param name="path">The location of the file where you want to save the document.</param>
        /// <param name="credentials"></param>
        /// <param name="method">The HTTP method used to open the connection, such as GET, POST, PUT, or PROPFIND.</param>
        /// <param name="proxy"></param>
        public void Get(string url, string path, WebProxy proxy, NetworkCredential credentials, string method)
        {
            var uri = new Uri(url);
            if (!(uri.Scheme == Uri.UriSchemeHttps) && !(uri.Scheme == Uri.UriSchemeHttp))
            {
                throw new HtmlWebException(string.Concat("Unsupported uri scheme: '", uri.Scheme, "'."));
            }
            Get(uri, method, path, null, proxy, credentials);
        }

        private HttpStatusCode Get(Uri uri, string method, string path, HtmlDocument doc, IWebProxy proxy,
            ICredentials creds)
        {
            HttpWebResponse resp;
            Encoding encoding;
            string cachePath = null;
            bool oldFile = false;
            var req = WebRequest.Create(uri) as HttpWebRequest;
            req.Method = method;
            req.UserAgent = UserAgent;
            if (proxy != null)
            {
                if (creds == null)
                {
                    proxy.Credentials = CredentialCache.DefaultCredentials;
                    req.Credentials = CredentialCache.DefaultCredentials;
                }
                else
                {
                    proxy.Credentials = creds;
                    req.Credentials = creds;
                }
                req.Proxy = proxy;
            }
            FromCache = false;
            RequestDuration = 0;
            int tc = Environment.TickCount;
            if (UsingCache)
            {
                cachePath = GetCachePath(req.RequestUri);
                if (File.Exists(cachePath))
                {
                    req.IfModifiedSince = File.GetLastAccessTime(cachePath);
                    oldFile = true;
                }
            }
            if (_cacheOnly)
            {
                if (!File.Exists(cachePath))
                {
                    throw new HtmlWebException(string.Concat("File was not found at cache path: '", cachePath, "'"));
                }
                if (path != null)
                {
                    IOLibrary.CopyAlways(cachePath, path);
                    if (cachePath != null)
                    {
                        File.SetLastWriteTime(path, File.GetLastWriteTime(cachePath));
                    }
                }
                FromCache = true;
                return HttpStatusCode.NotModified;
            }
            if (UseCookies)
            {
                req.CookieContainer = new CookieContainer();
            }
            if (PreRequest != null && !PreRequest(req))
            {
                return HttpStatusCode.ResetContent;
            }
            try
            {
                resp = req.GetResponse() as HttpWebResponse;
            }
            catch (WebException webException)
            {
                RequestDuration = Environment.TickCount - tc;
                resp = (HttpWebResponse) webException.Response;
                if (resp == null)
                {
                    if (!oldFile)
                    {
                        throw;
                    }
                    if (path != null)
                    {
                        IOLibrary.CopyAlways(cachePath, path);
                        File.SetLastWriteTime(path, File.GetLastWriteTime(cachePath));
                    }
                    return HttpStatusCode.NotModified;
                }
            }
            catch (Exception exception)
            {
                RequestDuration = Environment.TickCount - tc;
                throw;
            }
            if (PostResponse != null)
            {
                PostResponse(req, resp);
            }
            RequestDuration = Environment.TickCount - tc;
            ResponseUri = resp.ResponseUri;
            bool html = IsHtmlContent(resp.ContentType);
            if (!string.IsNullOrEmpty(resp.ContentEncoding))
            {
                encoding = Encoding.GetEncoding(resp.ContentEncoding);
            }
            else
            {
                encoding = null;
            }
            var respenc = encoding;
            if (OverrideEncoding != null)
            {
                respenc = OverrideEncoding;
            }
            if (resp.StatusCode == HttpStatusCode.NotModified)
            {
                if (!UsingCache)
                {
                    throw new HtmlWebException("Server has send a NotModifed code, without cache enabled.");
                }
                FromCache = true;
                if (path != null)
                {
                    IOLibrary.CopyAlways(cachePath, path);
                    File.SetLastWriteTime(path, File.GetLastWriteTime(cachePath));
                }
                return resp.StatusCode;
            }
            var s = resp.GetResponseStream();
            if (s != null)
            {
                if (UsingCache)
                {
                    SaveStream(s, cachePath, RemoveMilliseconds(resp.LastModified), _streamBufferSize);
                    SaveCacheHeaders(req.RequestUri, resp);
                    if (path != null)
                    {
                        IOLibrary.CopyAlways(cachePath, path);
                        File.SetLastWriteTime(path, File.GetLastWriteTime(cachePath));
                    }
                }
                else if (doc != null & html)
                {
                    if (respenc != null)
                    {
                        doc.Load(s, respenc);
                    }
                    else
                    {
                        doc.Load(s, true);
                    }
                }
                resp.Close();
            }
            return resp.StatusCode;
        }

        private string GetCacheHeader(Uri requestUri, string name, string def)
        {
            var xmlDocument = new XmlDocument();
            xmlDocument.Load(GetCacheHeadersPath(requestUri));
            var node =
                xmlDocument.SelectSingleNode(
                    string.Concat("//h[translate(@n, 'abcdefghijklmnopqrstuvwxyz','ABCDEFGHIJKLMNOPQRSTUVWXYZ')='",
                        name.ToUpper(), "']"));
            if (node == null)
            {
                return def;
            }
            return node.Attributes[name].Value;
        }

        private string GetCacheHeadersPath(Uri uri)
        {
            return string.Concat(GetCachePath(uri), ".h.xml");
        }

        /// <summary>
        ///     Gets the cache file path for a specified url.
        /// </summary>
        /// <param name="uri">The url fo which to retrieve the cache path. May not be null.</param>
        /// <returns>The cache file path.</returns>
        public string GetCachePath(Uri uri)
        {
            string cachePath;
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }
            if (!UsingCache)
            {
                throw new HtmlWebException("Cache is not enabled. Set UsingCache to true first.");
            }
            cachePath = uri.AbsolutePath != "/"
                ? Path.Combine(CachePath, string.Concat(uri.Host, uri.AbsolutePath).Replace('/', '\\'))
                : Path.Combine(CachePath, ".htm");
            return cachePath;
        }

        /// <summary>
        ///     Gets the MIME content type for a given path extension.
        /// </summary>
        /// <param name="extension">The input path extension.</param>
        /// <param name="def">The default content type to return if any error occurs.</param>
        /// <returns>The path extension's MIME content type.</returns>
        public static string GetContentTypeForExtension(string extension, string def)
        {
            var helper = new PermissionHelper();
            if (string.IsNullOrEmpty(extension))
            {
                return def;
            }
            string contentType = "";
            if (!helper.GetIsRegistryAvailable())
            {
                contentType = !MimeTypes.ContainsKey(extension) ? def : MimeTypes[extension];
            }
            if (!helper.GetIsDnsAvailable())
            {
                try
                {
                    var reg = Registry.ClassesRoot;
                    reg = reg.OpenSubKey(extension, false);
                    if (reg != null)
                    {
                        contentType = (string) reg.GetValue("", def);
                    }
                }
                catch (Exception exception)
                {
                    contentType = def;
                }
            }
            return contentType;
        }

        /// <summary>
        ///     Gets the path extension for a given MIME content type.
        /// </summary>
        /// <param name="contentType">The input MIME content type.</param>
        /// <param name="def">The default path extension to return if any error occurs.</param>
        /// <returns>The MIME content type's path extension.</returns>
        public static string GetExtensionForContentType(string contentType, string def)
        {
            string value;
            var helper = new PermissionHelper();
            if (string.IsNullOrEmpty(contentType))
            {
                return def;
            }
            string ext = "";
            if (helper.GetIsRegistryAvailable())
            {
                if (helper.GetIsRegistryAvailable())
                {
                    try
                    {
                        var reg = Registry.ClassesRoot;
                        reg = reg.OpenSubKey(string.Concat("MIME\\Database\\Content Type\\", contentType), false);
                        if (reg != null)
                        {
                            ext = (string) reg.GetValue("Extension", def);
                        }
                    }
                    catch (Exception exception)
                    {
                        ext = def;
                    }
                }
                return ext;
            }
            if (MimeTypes.ContainsValue(contentType))
            {
                var enumerator = MimeTypes.GetEnumerator();
                try
                {
                    while (enumerator.MoveNext())
                    {
                        var pair = enumerator.Current;
                        if (pair.Value != contentType)
                        {
                            continue;
                        }
                        value = pair.Value;
                        return value;
                    }
                    return def;
                }
                finally
                {
                    ((IDisposable) enumerator).Dispose();
                }
                return value;
            }
            return def;
        }

        private bool IsCacheHtmlContent(string path)
        {
            string ct = GetContentTypeForExtension(Path.GetExtension(path), null);
            return IsHtmlContent(ct);
        }

        private bool IsGZipEncoding(string contentEncoding)
        {
            return contentEncoding.ToLower().StartsWith("gzip");
        }

        private bool IsHtmlContent(string contentType)
        {
            return contentType.ToLower().StartsWith("text/html");
        }

        /// <summary>
        ///     Gets an HTML document from an Internet resource.
        /// </summary>
        /// <param name="url">The requested URL, such as "http://Myserver/Mypath/Myfile.asp".</param>
        /// <returns>A new HTML document.</returns>
        public HtmlDocument Load(string url)
        {
            return Load(url, "GET");
        }

        /// <summary>
        ///     Gets an HTML document from an Internet resource.
        /// </summary>
        /// <param name="url">The requested URL, such as "http://Myserver/Mypath/Myfile.asp".</param>
        /// <param name="proxyHost">Host to use for Proxy</param>
        /// <param name="proxyPort">Port the Proxy is on</param>
        /// <param name="userId">User Id for Authentication</param>
        /// <param name="password">Password for Authentication</param>
        /// <returns>A new HTML document.</returns>
        public HtmlDocument Load(string url, string proxyHost, int proxyPort, string userId, string password)
        {
            var myProxy = new WebProxy(proxyHost, proxyPort)
            {
                BypassProxyOnLocal = true
            };
            NetworkCredential myCreds = null;
            if (userId != null && password != null)
            {
                myCreds = new NetworkCredential(userId, password);
                var credentialCaches = new CredentialCache
                {
                    {myProxy.Address, "Basic", myCreds},
                    {myProxy.Address, "Digest", myCreds}
                };
            }
            return Load(url, "GET", myProxy, myCreds);
        }

        /// <summary>
        ///     Loads an HTML document from an Internet resource.
        /// </summary>
        /// <param name="url">The requested URL, such as "http://Myserver/Mypath/Myfile.asp".</param>
        /// <param name="method">The HTTP method used to open the connection, such as GET, POST, PUT, or PROPFIND.</param>
        /// <returns>A new HTML document.</returns>
        public HtmlDocument Load(string url, string method)
        {
            HtmlDocument doc;
            var uri = new Uri(url);
            if (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp)
            {
                doc = LoadUrl(uri, method, null, null);
            }
            else
            {
                if (uri.Scheme != Uri.UriSchemeFile)
                {
                    throw new HtmlWebException(string.Concat("Unsupported uri scheme: '", uri.Scheme, "'."));
                }
                doc = new HtmlDocument
                {
                    OptionAutoCloseOnEnd = false
                };
                doc.OptionAutoCloseOnEnd = true;
                if (OverrideEncoding == null)
                {
                    doc.DetectEncodingAndLoad(url, AutoDetectEncoding);
                }
                else
                {
                    doc.Load(url, OverrideEncoding);
                }
            }
            if (PreHandleDocument != null)
            {
                PreHandleDocument(doc);
            }
            return doc;
        }

        /// <summary>
        ///     Loads an HTML document from an Internet resource.
        /// </summary>
        /// <param name="url">The requested URL, such as "http://Myserver/Mypath/Myfile.asp".</param>
        /// <param name="method">The HTTP method used to open the connection, such as GET, POST, PUT, or PROPFIND.</param>
        /// <param name="proxy">Proxy to use with this request</param>
        /// <param name="credentials">Credentials to use when authenticating</param>
        /// <returns>A new HTML document.</returns>
        public HtmlDocument Load(string url, string method, WebProxy proxy, NetworkCredential credentials)
        {
            HtmlDocument doc;
            var uri = new Uri(url);
            if (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp)
            {
                doc = LoadUrl(uri, method, proxy, credentials);
            }
            else
            {
                if (uri.Scheme != Uri.UriSchemeFile)
                {
                    throw new HtmlWebException(string.Concat("Unsupported uri scheme: '", uri.Scheme, "'."));
                }
                doc = new HtmlDocument
                {
                    OptionAutoCloseOnEnd = false
                };
                doc.OptionAutoCloseOnEnd = true;
                doc.DetectEncodingAndLoad(url, AutoDetectEncoding);
            }
            if (PreHandleDocument != null)
            {
                PreHandleDocument(doc);
            }
            return doc;
        }

        /// <summary>
        ///     Loads an HTML document from an Internet resource and saves it to the specified XmlTextWriter, after an XSLT
        ///     transformation.
        /// </summary>
        /// <param name="htmlUrl">The requested URL, such as "http://Myserver/Mypath/Myfile.asp".</param>
        /// <param name="xsltUrl">The URL that specifies the XSLT stylesheet to load.</param>
        /// <param name="xsltArgs">An XsltArgumentList containing the namespace-qualified arguments used as input to the transform.</param>
        /// <param name="writer">The XmlTextWriter to which you want to save.</param>
        public void LoadHtmlAsXml(string htmlUrl, string xsltUrl, XsltArgumentList xsltArgs, XmlTextWriter writer)
        {
            LoadHtmlAsXml(htmlUrl, xsltUrl, xsltArgs, writer, null);
        }

        /// <summary>
        ///     Loads an HTML document from an Internet resource and saves it to the specified XmlTextWriter, after an XSLT
        ///     transformation.
        /// </summary>
        /// <param name="htmlUrl">The requested URL, such as "http://Myserver/Mypath/Myfile.asp". May not be null.</param>
        /// <param name="xsltUrl">The URL that specifies the XSLT stylesheet to load.</param>
        /// <param name="xsltArgs">An XsltArgumentList containing the namespace-qualified arguments used as input to the transform.</param>
        /// <param name="writer">The XmlTextWriter to which you want to save.</param>
        /// <param name="xmlPath">
        ///     A file path where the temporary XML before transformation will be saved. Mostly used for
        ///     debugging purposes.
        /// </param>
        public void LoadHtmlAsXml(string htmlUrl, string xsltUrl, XsltArgumentList xsltArgs, XmlTextWriter writer,
            string xmlPath)
        {
            if (htmlUrl == null)
            {
                throw new ArgumentNullException("htmlUrl");
            }
            var doc = Load(htmlUrl);
            if (xmlPath != null)
            {
                var w = new XmlTextWriter(xmlPath, doc.Encoding);
                doc.Save(w);
                w.Close();
            }
            if (xsltArgs == null)
            {
                xsltArgs = new XsltArgumentList();
            }
            xsltArgs.AddParam("url", "", htmlUrl);
            xsltArgs.AddParam("requestDuration", "", RequestDuration);
            xsltArgs.AddParam("fromCache", "", FromCache);
            var xslCompiledTransform = new XslCompiledTransform();
            xslCompiledTransform.Load(xsltUrl);
            xslCompiledTransform.Transform(doc, xsltArgs, writer);
        }

        /// <summary>
        ///     Loads an HTML document from an Internet resource and saves it to the specified XmlTextWriter.
        /// </summary>
        /// <param name="htmlUrl">The requested URL, such as "http://Myserver/Mypath/Myfile.asp".</param>
        /// <param name="writer">The XmlTextWriter to which you want to save to.</param>
        public void LoadHtmlAsXml(string htmlUrl, XmlTextWriter writer)
        {
            Load(htmlUrl).Save(writer);
        }

        private HtmlDocument LoadUrl(Uri uri, string method, WebProxy proxy, NetworkCredential creds)
        {
            var doc = new HtmlDocument
            {
                OptionAutoCloseOnEnd = false,
                OptionFixNestedTags = true
            };
            StatusCode = Get(uri, method, null, doc, proxy, creds);
            if (StatusCode == HttpStatusCode.NotModified)
            {
                doc.DetectEncodingAndLoad(GetCachePath(uri));
            }
            return doc;
        }

        private static DateTime RemoveMilliseconds(DateTime t)
        {
            return new DateTime(t.Year, t.Month, t.Day, t.Hour, t.Minute, t.Second, 0);
        }

        private void SaveCacheHeaders(Uri requestUri, HttpWebResponse resp)
        {
            string file = GetCacheHeadersPath(requestUri);
            var doc = new XmlDocument();
            doc.LoadXml("<c></c>");
            var cache = doc.FirstChild;
            foreach (string header in resp.Headers)
            {
                XmlNode entry = doc.CreateElement("h");
                var att = doc.CreateAttribute("n");
                att.Value = header;
                entry.Attributes.Append(att);
                att = doc.CreateAttribute("v");
                att.Value = resp.Headers[header];
                entry.Attributes.Append(att);
                cache.AppendChild(entry);
            }
            doc.Save(file);
        }

        private static long SaveStream(Stream stream, string path, DateTime touchDate, int streamBufferSize)
        {
            byte[] buffer;
            FilePreparePath(path);
            var fs = new FileStream(path, FileMode.Create, FileAccess.Write);
            BinaryReader br = null;
            BinaryWriter bw = null;
            long len = 0;
            try
            {
                br = new BinaryReader(stream);
                bw = new BinaryWriter(fs);
                do
                {
                    buffer = br.ReadBytes(streamBufferSize);
                    len = len + buffer.Length;
                    if (buffer.Length == 0)
                    {
                        continue;
                    }
                    bw.Write(buffer);
                } while (buffer.Length != 0);
            }
            finally
            {
                if (br != null)
                {
                    br.Close();
                }
                if (bw != null)
                {
                    bw.Flush();
                    bw.Close();
                }
                if (fs != null)
                {
                    fs.Close();
                }
            }
            File.SetLastWriteTime(path, touchDate);
            return len;
        }
    }
}