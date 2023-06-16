using System;
using System.Text;

public static class JsonTextParse
{
	public static string ToJsonText(string _text)
	{
		StringBuilder sb = new StringBuilder(_text.Length);
		int lastIdx = 0;
		for (int i = 0; i < _text.Length; ++i)
		{
			char ch = _text[i];
			if (ch == '\"' || ch == '\\')
			{
				var length = i - lastIdx;

				if (0 < length)
					sb.Append(_text.Substring(lastIdx, length));
				lastIdx = i + 1;

				sb.Append('%');
				sb.Append(((int)ch).ToString("X2"));
			}
		}
		if (lastIdx < _text.Length)
			sb.Append(_text.Substring(lastIdx, _text.Length - lastIdx));

		return sb.ToString();
	}

	public static string FromJsonText(string _text)
	{
		if (_text == null)
			return "";
		StringBuilder sb = new StringBuilder(_text.Length);
		for (int i = 0; i < _text.Length; ++i)
		{
			var ch = _text[i];
			if (ch == '%' && i < _text.Length - 2)
			{
				var tmp = _text.Substring(i + 1, 2);
				var value = -1;
				try { value = Convert.ToInt32(tmp, 16); }
				catch { }

				switch ((char)value)
				{
					case '\"': sb.Append('\"'); i += 2; continue;
					case '\\': sb.Append('\\'); i += 2; continue;
					default: break;
				}
			}
			sb.Append(ch);
		}

		return sb.ToString();
	}
}
