using System.Text;
using Common.Lang.Extensions;

namespace Common.Lang;

//  Custom character encoding to compress bytes array
//  https://stackoverflow.com/questions/47914529/how-do-i-shorten-and-expand-a-uuid-to-a-15-or-less-characters
//  http://aspell.net/charsets/codepages.html
public static class Base256
{
    public static byte[] FromBase256String(this string self)
    {
        return self.ToCharArray().Select(x => _lookup.GetOrDefault(x)).ToArray();
    }

    public static string ToBase256String(this byte[] self)
    {
        return new string(self.Select(x => _set[x]).ToArray());
    }

    private static readonly char[] _set = "©☺☻♥♦♣♠•◘◦◙♂♀♪♫☼▶◀↕‼¶§▬↨↑↓→←®↔▲▼ !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~⌘ÇüéâäàåçêëèïîìÄÅÉæÆôöòûùÿÖÜ¢£¥₧ƒáíóúñÑªº¿⌐¬½¼¡«»░▒▓│┤╡╢╖╕╣║╗╝╜╛┐└┴┬├─┼╞╟╚╔╩╦╠═╬╧╨╤╥╙╘╒╓╫╪┘┌█▄▌▐▀αβΓπΣσμτΦΘΩδ∞∅∈∩≡±≥≤⌠⌡÷≈°∙·√ⁿ²■¹".ToCharArray();
    private static readonly IDictionary<char, byte> _lookup = new Dictionary<char, byte>(_set.Select((x, i) => new KeyValuePair<char, byte>(x, (byte)i)));
}