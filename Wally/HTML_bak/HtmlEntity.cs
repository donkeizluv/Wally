using System;
using System.Collections.Generic;
using System.Text;

namespace Wally.HTML
{
    /// <summary>
    /// A utility class to replace special characters by entities and vice-versa.
    /// Follows HTML 4.0 specification found at http://www.w3.org/TR/html4/sgml/entities.html
    /// </summary>
    internal class HtmlEntity
    {
        private enum ParseState
        {
            Text,
            EntityStart
        }

        private static readonly int _maxEntitySize;

        /// <summary>
        /// A collection of entities indexed by name.
        /// </summary>
        public static Dictionary<int, string> EntityName { get; }

        /// <summary>
        /// A collection of entities indexed by value.
        /// </summary>
        public static Dictionary<string, int> EntityValue { get; }

        static HtmlEntity()
        {
            EntityName = new Dictionary<int, string>();
            EntityValue = new Dictionary<string, int>();
            EntityValue.Add("nbsp", 160);
            EntityName.Add(160, "nbsp");
            EntityValue.Add("iexcl", 161);
            EntityName.Add(161, "iexcl");
            EntityValue.Add("cent", 162);
            EntityName.Add(162, "cent");
            EntityValue.Add("pound", 163);
            EntityName.Add(163, "pound");
            EntityValue.Add("curren", 164);
            EntityName.Add(164, "curren");
            EntityValue.Add("yen", 165);
            EntityName.Add(165, "yen");
            EntityValue.Add("brvbar", 166);
            EntityName.Add(166, "brvbar");
            EntityValue.Add("sect", 167);
            EntityName.Add(167, "sect");
            EntityValue.Add("uml", 168);
            EntityName.Add(168, "uml");
            EntityValue.Add("copy", 169);
            EntityName.Add(169, "copy");
            EntityValue.Add("ordf", 170);
            EntityName.Add(170, "ordf");
            EntityValue.Add("laquo", 171);
            EntityName.Add(171, "laquo");
            EntityValue.Add("not", 172);
            EntityName.Add(172, "not");
            EntityValue.Add("shy", 173);
            EntityName.Add(173, "shy");
            EntityValue.Add("reg", 174);
            EntityName.Add(174, "reg");
            EntityValue.Add("macr", 175);
            EntityName.Add(175, "macr");
            EntityValue.Add("deg", 176);
            EntityName.Add(176, "deg");
            EntityValue.Add("plusmn", 177);
            EntityName.Add(177, "plusmn");
            EntityValue.Add("sup2", 178);
            EntityName.Add(178, "sup2");
            EntityValue.Add("sup3", 179);
            EntityName.Add(179, "sup3");
            EntityValue.Add("acute", 180);
            EntityName.Add(180, "acute");
            EntityValue.Add("micro", 181);
            EntityName.Add(181, "micro");
            EntityValue.Add("para", 182);
            EntityName.Add(182, "para");
            EntityValue.Add("middot", 183);
            EntityName.Add(183, "middot");
            EntityValue.Add("cedil", 184);
            EntityName.Add(184, "cedil");
            EntityValue.Add("sup1", 185);
            EntityName.Add(185, "sup1");
            EntityValue.Add("ordm", 186);
            EntityName.Add(186, "ordm");
            EntityValue.Add("raquo", 187);
            EntityName.Add(187, "raquo");
            EntityValue.Add("frac14", 188);
            EntityName.Add(188, "frac14");
            EntityValue.Add("frac12", 189);
            EntityName.Add(189, "frac12");
            EntityValue.Add("frac34", 190);
            EntityName.Add(190, "frac34");
            EntityValue.Add("iquest", 191);
            EntityName.Add(191, "iquest");
            EntityValue.Add("Agrave", 192);
            EntityName.Add(192, "Agrave");
            EntityValue.Add("Aacute", 193);
            EntityName.Add(193, "Aacute");
            EntityValue.Add("Acirc", 194);
            EntityName.Add(194, "Acirc");
            EntityValue.Add("Atilde", 195);
            EntityName.Add(195, "Atilde");
            EntityValue.Add("Auml", 196);
            EntityName.Add(196, "Auml");
            EntityValue.Add("Aring", 197);
            EntityName.Add(197, "Aring");
            EntityValue.Add("AElig", 198);
            EntityName.Add(198, "AElig");
            EntityValue.Add("Ccedil", 199);
            EntityName.Add(199, "Ccedil");
            EntityValue.Add("Egrave", 200);
            EntityName.Add(200, "Egrave");
            EntityValue.Add("Eacute", 201);
            EntityName.Add(201, "Eacute");
            EntityValue.Add("Ecirc", 202);
            EntityName.Add(202, "Ecirc");
            EntityValue.Add("Euml", 203);
            EntityName.Add(203, "Euml");
            EntityValue.Add("Igrave", 204);
            EntityName.Add(204, "Igrave");
            EntityValue.Add("Iacute", 205);
            EntityName.Add(205, "Iacute");
            EntityValue.Add("Icirc", 206);
            EntityName.Add(206, "Icirc");
            EntityValue.Add("Iuml", 207);
            EntityName.Add(207, "Iuml");
            EntityValue.Add("ETH", 208);
            EntityName.Add(208, "ETH");
            EntityValue.Add("Ntilde", 209);
            EntityName.Add(209, "Ntilde");
            EntityValue.Add("Ograve", 210);
            EntityName.Add(210, "Ograve");
            EntityValue.Add("Oacute", 211);
            EntityName.Add(211, "Oacute");
            EntityValue.Add("Ocirc", 212);
            EntityName.Add(212, "Ocirc");
            EntityValue.Add("Otilde", 213);
            EntityName.Add(213, "Otilde");
            EntityValue.Add("Ouml", 214);
            EntityName.Add(214, "Ouml");
            EntityValue.Add("times", 215);
            EntityName.Add(215, "times");
            EntityValue.Add("Oslash", 216);
            EntityName.Add(216, "Oslash");
            EntityValue.Add("Ugrave", 217);
            EntityName.Add(217, "Ugrave");
            EntityValue.Add("Uacute", 218);
            EntityName.Add(218, "Uacute");
            EntityValue.Add("Ucirc", 219);
            EntityName.Add(219, "Ucirc");
            EntityValue.Add("Uuml", 220);
            EntityName.Add(220, "Uuml");
            EntityValue.Add("Yacute", 221);
            EntityName.Add(221, "Yacute");
            EntityValue.Add("THORN", 222);
            EntityName.Add(222, "THORN");
            EntityValue.Add("szlig", 223);
            EntityName.Add(223, "szlig");
            EntityValue.Add("agrave", 224);
            EntityName.Add(224, "agrave");
            EntityValue.Add("aacute", 225);
            EntityName.Add(225, "aacute");
            EntityValue.Add("acirc", 226);
            EntityName.Add(226, "acirc");
            EntityValue.Add("atilde", 227);
            EntityName.Add(227, "atilde");
            EntityValue.Add("auml", 228);
            EntityName.Add(228, "auml");
            EntityValue.Add("aring", 229);
            EntityName.Add(229, "aring");
            EntityValue.Add("aelig", 230);
            EntityName.Add(230, "aelig");
            EntityValue.Add("ccedil", 231);
            EntityName.Add(231, "ccedil");
            EntityValue.Add("egrave", 232);
            EntityName.Add(232, "egrave");
            EntityValue.Add("eacute", 233);
            EntityName.Add(233, "eacute");
            EntityValue.Add("ecirc", 234);
            EntityName.Add(234, "ecirc");
            EntityValue.Add("euml", 235);
            EntityName.Add(235, "euml");
            EntityValue.Add("igrave", 236);
            EntityName.Add(236, "igrave");
            EntityValue.Add("iacute", 237);
            EntityName.Add(237, "iacute");
            EntityValue.Add("icirc", 238);
            EntityName.Add(238, "icirc");
            EntityValue.Add("iuml", 239);
            EntityName.Add(239, "iuml");
            EntityValue.Add("eth", 240);
            EntityName.Add(240, "eth");
            EntityValue.Add("ntilde", 241);
            EntityName.Add(241, "ntilde");
            EntityValue.Add("ograve", 242);
            EntityName.Add(242, "ograve");
            EntityValue.Add("oacute", 243);
            EntityName.Add(243, "oacute");
            EntityValue.Add("ocirc", 244);
            EntityName.Add(244, "ocirc");
            EntityValue.Add("otilde", 245);
            EntityName.Add(245, "otilde");
            EntityValue.Add("ouml", 246);
            EntityName.Add(246, "ouml");
            EntityValue.Add("divide", 247);
            EntityName.Add(247, "divide");
            EntityValue.Add("oslash", 248);
            EntityName.Add(248, "oslash");
            EntityValue.Add("ugrave", 249);
            EntityName.Add(249, "ugrave");
            EntityValue.Add("uacute", 250);
            EntityName.Add(250, "uacute");
            EntityValue.Add("ucirc", 251);
            EntityName.Add(251, "ucirc");
            EntityValue.Add("uuml", 252);
            EntityName.Add(252, "uuml");
            EntityValue.Add("yacute", 253);
            EntityName.Add(253, "yacute");
            EntityValue.Add("thorn", 254);
            EntityName.Add(254, "thorn");
            EntityValue.Add("yuml", 255);
            EntityName.Add(255, "yuml");
            EntityValue.Add("fnof", 402);
            EntityName.Add(402, "fnof");
            EntityValue.Add("Alpha", 913);
            EntityName.Add(913, "Alpha");
            EntityValue.Add("Beta", 914);
            EntityName.Add(914, "Beta");
            EntityValue.Add("Gamma", 915);
            EntityName.Add(915, "Gamma");
            EntityValue.Add("Delta", 916);
            EntityName.Add(916, "Delta");
            EntityValue.Add("Epsilon", 917);
            EntityName.Add(917, "Epsilon");
            EntityValue.Add("Zeta", 918);
            EntityName.Add(918, "Zeta");
            EntityValue.Add("Eta", 919);
            EntityName.Add(919, "Eta");
            EntityValue.Add("Theta", 920);
            EntityName.Add(920, "Theta");
            EntityValue.Add("Iota", 921);
            EntityName.Add(921, "Iota");
            EntityValue.Add("Kappa", 922);
            EntityName.Add(922, "Kappa");
            EntityValue.Add("Lambda", 923);
            EntityName.Add(923, "Lambda");
            EntityValue.Add("Mu", 924);
            EntityName.Add(924, "Mu");
            EntityValue.Add("Nu", 925);
            EntityName.Add(925, "Nu");
            EntityValue.Add("Xi", 926);
            EntityName.Add(926, "Xi");
            EntityValue.Add("Omicron", 927);
            EntityName.Add(927, "Omicron");
            EntityValue.Add("Pi", 928);
            EntityName.Add(928, "Pi");
            EntityValue.Add("Rho", 929);
            EntityName.Add(929, "Rho");
            EntityValue.Add("Sigma", 931);
            EntityName.Add(931, "Sigma");
            EntityValue.Add("Tau", 932);
            EntityName.Add(932, "Tau");
            EntityValue.Add("Upsilon", 933);
            EntityName.Add(933, "Upsilon");
            EntityValue.Add("Phi", 934);
            EntityName.Add(934, "Phi");
            EntityValue.Add("Chi", 935);
            EntityName.Add(935, "Chi");
            EntityValue.Add("Psi", 936);
            EntityName.Add(936, "Psi");
            EntityValue.Add("Omega", 937);
            EntityName.Add(937, "Omega");
            EntityValue.Add("alpha", 945);
            EntityName.Add(945, "alpha");
            EntityValue.Add("beta", 946);
            EntityName.Add(946, "beta");
            EntityValue.Add("gamma", 947);
            EntityName.Add(947, "gamma");
            EntityValue.Add("delta", 948);
            EntityName.Add(948, "delta");
            EntityValue.Add("epsilon", 949);
            EntityName.Add(949, "epsilon");
            EntityValue.Add("zeta", 950);
            EntityName.Add(950, "zeta");
            EntityValue.Add("eta", 951);
            EntityName.Add(951, "eta");
            EntityValue.Add("theta", 952);
            EntityName.Add(952, "theta");
            EntityValue.Add("iota", 953);
            EntityName.Add(953, "iota");
            EntityValue.Add("kappa", 954);
            EntityName.Add(954, "kappa");
            EntityValue.Add("lambda", 955);
            EntityName.Add(955, "lambda");
            EntityValue.Add("mu", 956);
            EntityName.Add(956, "mu");
            EntityValue.Add("nu", 957);
            EntityName.Add(957, "nu");
            EntityValue.Add("xi", 958);
            EntityName.Add(958, "xi");
            EntityValue.Add("omicron", 959);
            EntityName.Add(959, "omicron");
            EntityValue.Add("pi", 960);
            EntityName.Add(960, "pi");
            EntityValue.Add("rho", 961);
            EntityName.Add(961, "rho");
            EntityValue.Add("sigmaf", 962);
            EntityName.Add(962, "sigmaf");
            EntityValue.Add("sigma", 963);
            EntityName.Add(963, "sigma");
            EntityValue.Add("tau", 964);
            EntityName.Add(964, "tau");
            EntityValue.Add("upsilon", 965);
            EntityName.Add(965, "upsilon");
            EntityValue.Add("phi", 966);
            EntityName.Add(966, "phi");
            EntityValue.Add("chi", 967);
            EntityName.Add(967, "chi");
            EntityValue.Add("psi", 968);
            EntityName.Add(968, "psi");
            EntityValue.Add("omega", 969);
            EntityName.Add(969, "omega");
            EntityValue.Add("thetasym", 977);
            EntityName.Add(977, "thetasym");
            EntityValue.Add("upsih", 978);
            EntityName.Add(978, "upsih");
            EntityValue.Add("piv", 982);
            EntityName.Add(982, "piv");
            EntityValue.Add("bull", 8226);
            EntityName.Add(8226, "bull");
            EntityValue.Add("hellip", 8230);
            EntityName.Add(8230, "hellip");
            EntityValue.Add("prime", 8242);
            EntityName.Add(8242, "prime");
            EntityValue.Add("Prime", 8243);
            EntityName.Add(8243, "Prime");
            EntityValue.Add("oline", 8254);
            EntityName.Add(8254, "oline");
            EntityValue.Add("frasl", 8260);
            EntityName.Add(8260, "frasl");
            EntityValue.Add("weierp", 8472);
            EntityName.Add(8472, "weierp");
            EntityValue.Add("image", 8465);
            EntityName.Add(8465, "image");
            EntityValue.Add("real", 8476);
            EntityName.Add(8476, "real");
            EntityValue.Add("trade", 8482);
            EntityName.Add(8482, "trade");
            EntityValue.Add("alefsym", 8501);
            EntityName.Add(8501, "alefsym");
            EntityValue.Add("larr", 8592);
            EntityName.Add(8592, "larr");
            EntityValue.Add("uarr", 8593);
            EntityName.Add(8593, "uarr");
            EntityValue.Add("rarr", 8594);
            EntityName.Add(8594, "rarr");
            EntityValue.Add("darr", 8595);
            EntityName.Add(8595, "darr");
            EntityValue.Add("harr", 8596);
            EntityName.Add(8596, "harr");
            EntityValue.Add("crarr", 8629);
            EntityName.Add(8629, "crarr");
            EntityValue.Add("lArr", 8656);
            EntityName.Add(8656, "lArr");
            EntityValue.Add("uArr", 8657);
            EntityName.Add(8657, "uArr");
            EntityValue.Add("rArr", 8658);
            EntityName.Add(8658, "rArr");
            EntityValue.Add("dArr", 8659);
            EntityName.Add(8659, "dArr");
            EntityValue.Add("hArr", 8660);
            EntityName.Add(8660, "hArr");
            EntityValue.Add("forall", 8704);
            EntityName.Add(8704, "forall");
            EntityValue.Add("part", 8706);
            EntityName.Add(8706, "part");
            EntityValue.Add("exist", 8707);
            EntityName.Add(8707, "exist");
            EntityValue.Add("empty", 8709);
            EntityName.Add(8709, "empty");
            EntityValue.Add("nabla", 8711);
            EntityName.Add(8711, "nabla");
            EntityValue.Add("isin", 8712);
            EntityName.Add(8712, "isin");
            EntityValue.Add("notin", 8713);
            EntityName.Add(8713, "notin");
            EntityValue.Add("ni", 8715);
            EntityName.Add(8715, "ni");
            EntityValue.Add("prod", 8719);
            EntityName.Add(8719, "prod");
            EntityValue.Add("sum", 8721);
            EntityName.Add(8721, "sum");
            EntityValue.Add("minus", 8722);
            EntityName.Add(8722, "minus");
            EntityValue.Add("lowast", 8727);
            EntityName.Add(8727, "lowast");
            EntityValue.Add("radic", 8730);
            EntityName.Add(8730, "radic");
            EntityValue.Add("prop", 8733);
            EntityName.Add(8733, "prop");
            EntityValue.Add("infin", 8734);
            EntityName.Add(8734, "infin");
            EntityValue.Add("ang", 8736);
            EntityName.Add(8736, "ang");
            EntityValue.Add("and", 8743);
            EntityName.Add(8743, "and");
            EntityValue.Add("or", 8744);
            EntityName.Add(8744, "or");
            EntityValue.Add("cap", 8745);
            EntityName.Add(8745, "cap");
            EntityValue.Add("cup", 8746);
            EntityName.Add(8746, "cup");
            EntityValue.Add("int", 8747);
            EntityName.Add(8747, "int");
            EntityValue.Add("there4", 8756);
            EntityName.Add(8756, "there4");
            EntityValue.Add("sim", 8764);
            EntityName.Add(8764, "sim");
            EntityValue.Add("cong", 8773);
            EntityName.Add(8773, "cong");
            EntityValue.Add("asymp", 8776);
            EntityName.Add(8776, "asymp");
            EntityValue.Add("ne", 8800);
            EntityName.Add(8800, "ne");
            EntityValue.Add("equiv", 8801);
            EntityName.Add(8801, "equiv");
            EntityValue.Add("le", 8804);
            EntityName.Add(8804, "le");
            EntityValue.Add("ge", 8805);
            EntityName.Add(8805, "ge");
            EntityValue.Add("sub", 8834);
            EntityName.Add(8834, "sub");
            EntityValue.Add("sup", 8835);
            EntityName.Add(8835, "sup");
            EntityValue.Add("nsub", 8836);
            EntityName.Add(8836, "nsub");
            EntityValue.Add("sube", 8838);
            EntityName.Add(8838, "sube");
            EntityValue.Add("supe", 8839);
            EntityName.Add(8839, "supe");
            EntityValue.Add("oplus", 8853);
            EntityName.Add(8853, "oplus");
            EntityValue.Add("otimes", 8855);
            EntityName.Add(8855, "otimes");
            EntityValue.Add("perp", 8869);
            EntityName.Add(8869, "perp");
            EntityValue.Add("sdot", 8901);
            EntityName.Add(8901, "sdot");
            EntityValue.Add("lceil", 8968);
            EntityName.Add(8968, "lceil");
            EntityValue.Add("rceil", 8969);
            EntityName.Add(8969, "rceil");
            EntityValue.Add("lfloor", 8970);
            EntityName.Add(8970, "lfloor");
            EntityValue.Add("rfloor", 8971);
            EntityName.Add(8971, "rfloor");
            EntityValue.Add("lang", 9001);
            EntityName.Add(9001, "lang");
            EntityValue.Add("rang", 9002);
            EntityName.Add(9002, "rang");
            EntityValue.Add("loz", 9674);
            EntityName.Add(9674, "loz");
            EntityValue.Add("spades", 9824);
            EntityName.Add(9824, "spades");
            EntityValue.Add("clubs", 9827);
            EntityName.Add(9827, "clubs");
            EntityValue.Add("hearts", 9829);
            EntityName.Add(9829, "hearts");
            EntityValue.Add("diams", 9830);
            EntityName.Add(9830, "diams");
            EntityValue.Add("quot", 34);
            EntityName.Add(34, "quot");
            EntityValue.Add("amp", 38);
            EntityName.Add(38, "amp");
            EntityValue.Add("lt", 60);
            EntityName.Add(60, "lt");
            EntityValue.Add("gt", 62);
            EntityName.Add(62, "gt");
            EntityValue.Add("OElig", 338);
            EntityName.Add(338, "OElig");
            EntityValue.Add("oelig", 339);
            EntityName.Add(339, "oelig");
            EntityValue.Add("Scaron", 352);
            EntityName.Add(352, "Scaron");
            EntityValue.Add("scaron", 353);
            EntityName.Add(353, "scaron");
            EntityValue.Add("Yuml", 376);
            EntityName.Add(376, "Yuml");
            EntityValue.Add("circ", 710);
            EntityName.Add(710, "circ");
            EntityValue.Add("tilde", 732);
            EntityName.Add(732, "tilde");
            EntityValue.Add("ensp", 8194);
            EntityName.Add(8194, "ensp");
            EntityValue.Add("emsp", 8195);
            EntityName.Add(8195, "emsp");
            EntityValue.Add("thinsp", 8201);
            EntityName.Add(8201, "thinsp");
            EntityValue.Add("zwnj", 8204);
            EntityName.Add(8204, "zwnj");
            EntityValue.Add("zwj", 8205);
            EntityName.Add(8205, "zwj");
            EntityValue.Add("lrm", 8206);
            EntityName.Add(8206, "lrm");
            EntityValue.Add("rlm", 8207);
            EntityName.Add(8207, "rlm");
            EntityValue.Add("ndash", 8211);
            EntityName.Add(8211, "ndash");
            EntityValue.Add("mdash", 8212);
            EntityName.Add(8212, "mdash");
            EntityValue.Add("lsquo", 8216);
            EntityName.Add(8216, "lsquo");
            EntityValue.Add("rsquo", 8217);
            EntityName.Add(8217, "rsquo");
            EntityValue.Add("sbquo", 8218);
            EntityName.Add(8218, "sbquo");
            EntityValue.Add("ldquo", 8220);
            EntityName.Add(8220, "ldquo");
            EntityValue.Add("rdquo", 8221);
            EntityName.Add(8221, "rdquo");
            EntityValue.Add("bdquo", 8222);
            EntityName.Add(8222, "bdquo");
            EntityValue.Add("dagger", 8224);
            EntityName.Add(8224, "dagger");
            EntityValue.Add("Dagger", 8225);
            EntityName.Add(8225, "Dagger");
            EntityValue.Add("permil", 8240);
            EntityName.Add(8240, "permil");
            EntityValue.Add("lsaquo", 8249);
            EntityName.Add(8249, "lsaquo");
            EntityValue.Add("rsaquo", 8250);
            EntityName.Add(8250, "rsaquo");
            EntityValue.Add("euro", 8364);
            EntityName.Add(8364, "euro");
            _maxEntitySize = 9;
        }

        private HtmlEntity()
        {
        }

        /// <summary>
        /// Replace known entities by characters.
        /// </summary>
        /// <param name="text">The source text.</param>
        /// <returns>The result text.</returns>
        public static string DeEntitize(string text)
        {
            if (text == null)
            {
                return null;
            }
            if (text.Length == 0)
            {
                return text;
            }
            StringBuilder sb = new StringBuilder(text.Length);
            ParseState state = ParseState.Text;
            StringBuilder entity = new StringBuilder(10);
            for (int i = 0; i < text.Length; i++)
            {
                switch (state)
                {
                    case ParseState.Text:
                    {
                        char c = text[i];
                        if (c == '&')
                        {
                            state = ParseState.EntityStart;
                        }
                        else
                        {
                            sb.Append(text[i]);
                        }
                        break;
                    }
                    case ParseState.EntityStart:
                    {
                        char c2 = text[i];
                        if (c2 != '&')
                        {
                            if (c2 == ';')
                            {
                                if (entity.Length == 0)
                                {
                                    sb.Append("&;");
                                }
                                else
                                {
                                    if (entity[0] == '#')
                                    {
                                        string e = entity.ToString();
                                        try
                                        {
                                            string codeStr = e.Substring(1).Trim().ToLower();
                                            int fromBase;
                                            if (codeStr.StartsWith("x"))
                                            {
                                                fromBase = 16;
                                                codeStr = codeStr.Substring(1);
                                            }
                                            else
                                            {
                                                fromBase = 10;
                                            }
                                            int code = Convert.ToInt32(codeStr, fromBase);
                                            sb.Append(Convert.ToChar(code));
                                            goto IL_16A;
                                        }
                                        catch
                                        {
                                            sb.Append("&#" + e + ";");
                                            goto IL_16A;
                                        }
                                        goto IL_11F;
                                    }
                                    goto IL_11F;
                                    IL_16A:
                                    entity.Remove(0, entity.Length);
                                    goto IL_178;
                                    IL_11F:
                                    object o = EntityValue[entity.ToString()];
                                    if (o == null)
                                    {
                                        sb.Append("&" + entity + ";");
                                        goto IL_16A;
                                    }
                                    int code2 = (int) o;
                                    sb.Append(Convert.ToChar(code2));
                                    goto IL_16A;
                                }
                                IL_178:
                                state = ParseState.Text;
                            }
                            else
                            {
                                entity.Append(text[i]);
                                if (entity.Length > _maxEntitySize)
                                {
                                    state = ParseState.Text;
                                    sb.Append("&" + entity);
                                    entity.Remove(0, entity.Length);
                                }
                            }
                        }
                        else
                        {
                            sb.Append("&" + entity);
                            entity.Remove(0, entity.Length);
                        }
                        break;
                    }
                }
            }
            if (state == ParseState.EntityStart)
            {
                sb.Append("&" + entity);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Clone and entitize an HtmlNode. This will affect attribute values and nodes' text. It will also entitize all child nodes.
        /// </summary>
        /// <param name="node">The node to entitize.</param>
        /// <returns>An entitized cloned node.</returns>
        public static HtmlNode Entitize(HtmlNode node)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }
            HtmlNode result = node.CloneNode(true);
            if (result.HasAttributes)
            {
                Entitize(result.Attributes);
            }
            if (result.HasChildNodes)
            {
                Entitize(result.ChildNodes);
            }
            else if (result.NodeType == HtmlNodeType.Text)
            {
                ((HtmlTextNode) result).Text = Entitize(((HtmlTextNode) result).Text, true, true);
            }
            return result;
        }

        /// <summary>
        /// Replace characters above 127 by entities.
        /// </summary>
        /// <param name="text">The source text.</param>
        /// <returns>The result text.</returns>
        public static string Entitize(string text)
        {
            return Entitize(text, true);
        }

        /// <summary>
        /// Replace characters above 127 by entities.
        /// </summary>
        /// <param name="text">The source text.</param>
        /// <param name="useNames">If set to false, the function will not use known entities name. Default is true.</param>
        /// <returns>The result text.</returns>
        public static string Entitize(string text, bool useNames)
        {
            return Entitize(text, useNames, false);
        }

        /// <summary>
        /// Replace characters above 127 by entities.
        /// </summary>
        /// <param name="text">The source text.</param>
        /// <param name="useNames">If set to false, the function will not use known entities name. Default is true.</param>
        /// <param name="entitizeQuotAmpAndLtGt">If set to true, the [quote], [ampersand], [lower than] and [greather than] characters will be entitized.</param>
        /// <returns>The result text</returns>
        public static string Entitize(string text, bool useNames, bool entitizeQuotAmpAndLtGt)
        {
            if (text == null)
            {
                return null;
            }
            if (text.Length == 0)
            {
                return text;
            }
            StringBuilder sb = new StringBuilder(text.Length);
            for (int i = 0; i < text.Length; i++)
            {
                int code = text[i];
                if (code > 127 || (entitizeQuotAmpAndLtGt && (code == 34 || code == 38 || code == 60 || code == 62)))
                {
                    string entity = EntityName[code];
                    if (entity == null || !useNames)
                    {
                        sb.Append("&#" + code + ";");
                    }
                    else
                    {
                        sb.Append("&" + entity + ";");
                    }
                }
                else
                {
                    sb.Append(text[i]);
                }
            }
            return sb.ToString();
        }

        private static void Entitize(HtmlAttributeCollection collection)
        {
            foreach (HtmlAttribute at in collection)
            {
                at.Value = Entitize(at.Value);
            }
        }

        private static void Entitize(HtmlNodeCollection collection)
        {
            foreach (HtmlNode node in collection)
            {
                if (node.HasAttributes)
                {
                    Entitize(node.Attributes);
                }
                if (node.HasChildNodes)
                {
                    Entitize(node.ChildNodes);
                }
                else if (node.NodeType == HtmlNodeType.Text)
                {
                    ((HtmlTextNode) node).Text = Entitize(((HtmlTextNode) node).Text, true, true);
                }
            }
        }
    }
}