namespace Common.Serialization.Xml
{
    public static class XmlExtensions
    {
        public static T ParseXmlAs<T>(this string self, IXmlizer? parser = null)
        {
            parser ??= SysXmlizer.Instance;
            return parser.Deserialize<T>(self);
        }

        public static T ParseXmlAs<T>(this string self, Type type, IXmlizer? parser = null)
        {
            parser ??= SysXmlizer.Instance;
            return parser.Deserialize<T>(self, type);
        }

        public static Task<T> ParseXmlAs<T>(this Stream self, IXmlizer? parser = null, CancellationToken token = default)
        {
            parser ??= SysXmlizer.Instance;
            return parser.Deserialize<T>(self, token);
        }

        public static Task<T> ParseXmlAs<T>(this Stream self, Type type, IXmlizer? parser = null, CancellationToken token = default)
        {
            parser ??= SysXmlizer.Instance;
            return parser.Deserialize<T>(self, type, token);
        }

        public static string ComposeXml<T>(this T self, IXmlizer? composer = null)
        {
            composer ??= SysXmlizer.Instance;
            return composer.Serialize(self);
        }
    }
}