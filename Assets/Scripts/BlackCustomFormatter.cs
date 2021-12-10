using System;

public class BlackCustomFormatter : IFormatProvider, ICustomFormatter
{
    public object GetFormat(Type formatType)
    {
        if (formatType == typeof(ICustomFormatter))
        {
            return this;
        }
        else
        {
            return null;
        }
    }

    public string Format(string format, object arg, IFormatProvider formatProvider)
    {
        if (Equals(formatProvider) == false)
        {
            return null;
        }

        if (format == "msec3")
        {
            if (arg is IFormattable)
            {
                return ((long.Parse(arg.ToString())) / 1000.0f).ToString("F3");
            }
        }
        else if (format == "usec3")
        {
            if (arg is IFormattable)
            {
                var usecLong = long.Parse(arg.ToString());
                if (usecLong < 1000)
                {
                    // 0.001초보다 작아져서 0.000으로 나오는 경우에는
                    // 역수로 바꿔서 보여준다. 즉 xx초/개 형태가 아니라 xx개/초 형태로 보이는 것이다.
                    return (1.0f / (usecLong / 1000000.0f)).ToString("n0");
                }
                else
                {
                    return (usecLong / 1000000.0f).ToString("F3");
                }
            }
        }
        else if (format == "plural_s")
        {
            // 0 gem[s]
            // 1 gem[ ]
            // 2 gem[s]
            if (arg is IFormattable || arg is string)
            {
                var argStr = arg.ToString();
                return (argStr == "1" || argStr == "1.000") ? "" : "s";
            }
        }
        else if (format == "plural_yies")
        {
            // 0 energ[ies]
            // 1 energ[y  ]
            // 2 energ[ies]
            if (arg is IFormattable || arg is string)
            {
                return arg.ToString() == "1" ? "y" : "ies";
            }
        }
        else if (format == "plural_isare")
        {
            // 0 gems are needed
            // 1 gem is needed
            if (arg is IFormattable || arg is string)
            {
                return arg.ToString() == "1" ? "is" : "are";
            }
        }

        if (arg is IFormattable)
        {
            var formattableArg = arg as IFormattable;
            return formattableArg.ToString(format, null);
        }
        else
        {
            return arg.ToString();
        }
    }
}