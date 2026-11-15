using System.Globalization;
using System.Text.Json.Nodes;

namespace TKCode.Arc.A2UI.Chat;

/// <summary>
/// The agent's tools. They run locally and return demo data (stated in their descriptions), so examples work without
/// any backend: time, arithmetic, flights, weather, stock quotes, products.
/// </summary>
public static class ChatTools
{
    private static JsonObject Schema(params (string Name, string Type, string Description, bool Required)[] properties)
    {
        var props = new JsonObject();
        var required = new JsonArray();
        foreach (var (name, type, description, isRequired) in properties)
        {
            props[name] = new JsonObject { ["type"] = type, ["description"] = description };
            if (isRequired)
            {
                required.Add((JsonNode?)(name));
            }
        }

        return new JsonObject { ["type"] = "object", ["properties"] = props, ["required"] = required, ["additionalProperties"] = false };
    }

    public static IReadOnlyList<ChatTool> All { get; } =
    [
        new("get_current_time", "The current date and time (ISO 8601) and the user's time zone.", Schema()),
        new("calculate", "Evaluates an arithmetic expression with + - * / ( ) and decimals. Returns the number.",
            Schema(("expression", "string", "For example (12.5 * 4) / 3", true))),
        new("search_flights", "Demo data: flights between two cities on a date (made-up schedule and prices).",
            Schema(("from", "string", "Departure city", true), ("to", "string", "Arrival city", true), ("date", "string", "YYYY-MM-DD", false))),
        new("get_weather", "Demo data: current weather and a 5-day forecast for a city (made-up values).",
            Schema(("city", "string", "City name", true))),
        new("get_stock_quote", "Demo data: a stock's price and its last 30 daily closes (made-up values).",
            Schema(("symbol", "string", "Ticker symbol", true))),
        new("list_products", "Demo data: products of a small shop in a category (made-up catalog).",
            Schema(("category", "string", "laptops, phones, audio or accessories", false)))
    ];

    /// <summary>Runs a tool; returns its JSON result text, or an error message with <paramref name="isError"/>.</summary>
    public static string Run(string name, string argumentsJson, out bool isError)
    {
        isError = false;
        JsonObject args;
        try
        {
            args = A2UIJson.Parse(argumentsJson) as JsonObject ?? new JsonObject();
        }
        catch (A2UIException)
        {
            isError = true;
            return "The tool arguments are not valid JSON.";
        }

        string Arg(string key) => A2UIJson.AsString(args[key]) ?? string.Empty;
        try
        {
            return name switch
            {
                "get_current_time" => new JsonObject
                {
                    ["now"] = DateTimeOffset.Now.ToString("yyyy-MM-dd'T'HH:mm:sszzz", CultureInfo.InvariantCulture),
                    ["timeZone"] = TimeZoneInfo.Local.Id
                }.ToJsonString(),
                "calculate" => A2UIJson.FormatNumber(Calculator.Evaluate(Arg("expression"))),
                "search_flights" => Flights(Arg("from"), Arg("to"), Arg("date")).ToJsonString(),
                "get_weather" => Weather(Arg("city")).ToJsonString(),
                "get_stock_quote" => Stock(Arg("symbol")).ToJsonString(),
                "list_products" => Products(Arg("category")).ToJsonString(),
                _ => throw new InvalidOperationException($"Unknown tool '{name}'.")
            };
        }
        catch (Exception ex) when (ex is FormatException or InvalidOperationException or DivideByZeroException)
        {
            isError = true;
            return ex.Message;
        }
    }

    private static int Seed(string text) => text.Aggregate(17, (h, c) => unchecked(h * 31 + char.ToLowerInvariant(c)));

    private static JsonObject Flights(string from, string to, string date)
    {
        var random = new Random(Seed(from + to + date));
        var airlines = new[] { "Northwind Air", "Contoso Airlines", "Fabrikam Jet", "Litware Wings" };
        var flights = new JsonArray();
        for (var i = 0; i < 4; i++)
        {
            var depart = 6 + i * 4 + random.Next(0, 3);
            var duration = 90 + random.Next(0, 240);
            flights.Add((JsonNode?)(new JsonObject
            {
                ["id"] = $"{(char)('A' + i)}{random.Next(100, 999)}",
                ["airline"] = airlines[i % airlines.Length],
                ["depart"] = $"{depart:00}:{random.Next(0, 4) * 15:00}",
                ["durationMinutes"] = duration,
                ["stops"] = random.Next(0, 3) == 0 ? 1 : 0,
                ["price"] = 120 + random.Next(0, 600),
                ["currency"] = "USD"
            }));
        }

        return new JsonObject { ["from"] = from, ["to"] = to, ["date"] = date, ["flights"] = flights, ["note"] = "Demo data" };
    }

    private static JsonObject Weather(string city)
    {
        var random = new Random(Seed(city));
        var conditions = new[] { "Sunny", "Cloudy", "Rain", "Windy", "Snow" };
        var forecast = new JsonArray();
        var today = DateOnly.FromDateTime(DateTime.Today);
        for (var i = 0; i < 5; i++)
        {
            var high = 8 + random.Next(0, 22);
            forecast.Add((JsonNode?)(new JsonObject
            {
                ["date"] = today.AddDays(i).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                ["high"] = high,
                ["low"] = high - random.Next(4, 10),
                ["condition"] = conditions[random.Next(conditions.Length)]
            }));
        }

        return new JsonObject { ["city"] = city, ["temperature"] = forecast[0]!["high"]!.DeepClone(), ["unit"] = "C", ["forecast"] = forecast, ["note"] = "Demo data" };
    }

    private static JsonObject Stock(string symbol)
    {
        var random = new Random(Seed(symbol));
        var price = 50.0 + random.Next(0, 400);
        var closes = new JsonArray();
        for (var i = 0; i < 30; i++)
        {
            price = Math.Max(1, price * (1 + (random.NextDouble() - 0.48) * 0.04));
            closes.Add((JsonNode?)(Math.Round(price, 2)));
        }

        return new JsonObject { ["symbol"] = symbol.ToUpperInvariant(), ["price"] = Math.Round(price, 2), ["currency"] = "USD", ["closes"] = closes, ["note"] = "Demo data" };
    }

    private static JsonObject Products(string category)
    {
        var items = category.ToLowerInvariant() switch
        {
            "phones" => new[] { ("Pocket 12", 699), ("Pocket 12 Pro", 999), ("Fold Air", 1499) },
            "audio" => new[] { ("Studio Buds", 149), ("Over-Ear 2", 299), ("Room Speaker", 249) },
            "accessories" => new[] { ("USB-C Hub", 59), ("Laptop Sleeve", 39), ("Charger 65W", 49) },
            _ => new[] { ("Note 14", 1199), ("Note 16 Pro", 1899), ("Air 13", 999) }
        };
        var list = new JsonArray();
        foreach (var (name, price) in items)
        {
            list.Add((JsonNode?)(new JsonObject { ["name"] = name, ["price"] = price, ["currency"] = "USD", ["rating"] = 3.8 + Seed(name) % 12 / 10.0 }));
        }

        return new JsonObject { ["category"] = category, ["products"] = list, ["note"] = "Demo data" };
    }

    /// <summary>A small recursive-descent calculator: numbers, + - * /, parentheses, unary minus. No code is executed.</summary>
    internal static class Calculator
    {
        public static double Evaluate(string expression)
        {
            var pos = 0;
            var value = Sum(expression, ref pos);
            Skip(expression, ref pos);
            if (pos != expression.Length)
            {
                throw new FormatException($"Unexpected '{expression[pos]}' at {pos}.");
            }

            return value;
        }

        private static double Sum(string s, ref int pos)
        {
            var value = Product(s, ref pos);
            while (true)
            {
                Skip(s, ref pos);
                if (pos < s.Length && s[pos] is '+' or '-')
                {
                    var op = s[pos++];
                    var right = Product(s, ref pos);
                    value = op == '+' ? value + right : value - right;
                }
                else
                {
                    return value;
                }
            }
        }

        private static double Product(string s, ref int pos)
        {
            var value = Unary(s, ref pos);
            while (true)
            {
                Skip(s, ref pos);
                if (pos < s.Length && s[pos] is '*' or '/')
                {
                    var op = s[pos++];
                    var right = Unary(s, ref pos);
                    if (op == '/' && right == 0)
                    {
                        throw new DivideByZeroException("Division by zero.");
                    }

                    value = op == '*' ? value * right : value / right;
                }
                else
                {
                    return value;
                }
            }
        }

        private static double Unary(string s, ref int pos)
        {
            Skip(s, ref pos);
            if (pos < s.Length && s[pos] == '-')
            {
                pos++;
                return -Unary(s, ref pos);
            }

            if (pos < s.Length && s[pos] == '(')
            {
                pos++;
                var value = Sum(s, ref pos);
                Skip(s, ref pos);
                if (pos >= s.Length || s[pos] != ')')
                {
                    throw new FormatException("Missing ')'.");
                }

                pos++;
                return value;
            }

            var start = pos;
            while (pos < s.Length && (char.IsAsciiDigit(s[pos]) || s[pos] == '.'))
            {
                pos++;
            }

            if (start == pos || !double.TryParse(s.AsSpan(start, pos - start), NumberStyles.Float, CultureInfo.InvariantCulture, out var number))
            {
                throw new FormatException($"Expected a number at {start}.");
            }

            return number;
        }

        private static void Skip(string s, ref int pos)
        {
            while (pos < s.Length && char.IsWhiteSpace(s[pos]))
            {
                pos++;
            }
        }
    }
}
