namespace TKCode.Arc.Gallery.Demos;

/// <summary>
/// Synthetic hourly wind readings for the wind rose demos: a made-up harbour station where a south-westerly sea wind
/// dominates, a westerly follows, a light north-easterly land breeze blows at night, and winter brings northerly gusts.
/// A fixed seed and our own generator make every run (and every platform) draw the same rose.
/// </summary>
internal static class WindRoseDemoData
{
    /// <summary>Hours in each month of a non-leap year.</summary>
    private static readonly int[] s_hours = [744, 672, 744, 720, 744, 720, 744, 744, 720, 744, 720, 744];

    /// <summary>The month names the live demo cycles through.</summary>
    public static readonly string[] MonthNames =
        ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"];

    /// <summary>A whole year of hourly readings (8,760).</summary>
    public static WindObservation[] Year(int seed = 2025)
    {
        var all = new List<WindObservation>(8760);
        for (var month = 0; month < 12; month++)
        {
            all.AddRange(Month(month, seed));
        }
        return all.ToArray();
    }

    /// <summary>One month of hourly readings; <paramref name="month"/> is 0 for January.</summary>
    public static WindObservation[] Month(int month, int seed = 2025)
    {
        var random = new Rng((ulong)(seed * 31 + month + 1));
        // 1 in winter, 0 in summer: winter shifts weight to the northerly and north-easterly regimes and blows harder.
        var winter = (Math.Cos(month / 12.0 * 2 * Math.PI) + 1) / 2;
        Regime[] regimes =
        [
            new(225, 22, 6.2 + 1.2 * winter, 2.0, 0.40 - 0.10 * winter), // south-westerly sea wind
            new(275, 18, 5.2 + 1.0 * winter, 2.0, 0.20),                 // westerly
            new(45, 20, 3.4 + 0.8 * winter, 2.2, 0.16 + 0.06 * winter),  // north-easterly land breeze
            new(160, 28, 3.0, 1.8, 0.14 - 0.06 * winter),                // south-south-easterly
            new(355, 15, 4.0 + 3.0 * winter, 1.9, 0.04 + 0.10 * winter)  // northerly, gusty in winter
        ];
        var total = regimes.Sum(r => r.Weight);
        var readings = new WindObservation[s_hours[month]];
        for (var i = 0; i < readings.Length; i++)
        {
            // About one hour in twenty is calm, a little more in summer.
            if (random.Next() < 0.04 + 0.025 * (1 - winter))
            {
                readings[i] = new WindObservation(0, random.Next() * 0.45);
                continue;
            }
            var pick = random.Next() * total;
            var regime = regimes[^1];
            foreach (var candidate in regimes)
            {
                if (pick < candidate.Weight)
                {
                    regime = candidate;
                    break;
                }
                pick -= candidate.Weight;
            }
            var direction = regime.Direction + regime.Spread * random.Normal();
            var speed = regime.Scale * Math.Pow(-Math.Log(1 - random.Next()), 1 / regime.Shape);
            readings[i] = new WindObservation((direction % 360 + 360) % 360, Math.Round(speed, 1));
        }
        return readings;
    }

    private readonly record struct Regime(double Direction, double Spread, double Scale, double Shape, double Weight);

    /// <summary>xorshift64: small, fast and the same everywhere.</summary>
    private struct Rng(ulong seed)
    {
        private ulong _state = seed * 0x9E3779B97F4A7C15UL + 0x632BE59BD9B4E019UL;

        public double Next()
        {
            _state ^= _state << 13;
            _state ^= _state >> 7;
            _state ^= _state << 17;
            return (_state >> 11) * (1.0 / (1UL << 53));
        }

        public double Normal()
        {
            var u = 1 - Next();
            return Math.Sqrt(-2 * Math.Log(u)) * Math.Cos(2 * Math.PI * Next());
        }
    }
}
