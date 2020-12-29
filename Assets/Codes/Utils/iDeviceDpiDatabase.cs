using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace BanGround.Utils
{
    public class iDeviceDpiDatabase
    {
        public static int FindDpi(string modelString)
        {
            var db = new iDeviceDpiDatabase();

            var typeMatch = Regex.Match(modelString, "(i[\\w]*?)\\d");
            var genMatch = Regex.Match(modelString, "(\\d*),");
            var modelMatch = Regex.Match(modelString, ",(\\d*)");

            if (!typeMatch.Success || !genMatch.Success || !modelMatch.Success)
                throw new KeyNotFoundException("Not a vaild iDevice");

            var type = typeMatch.Groups[1].Value;
            var gen = int.Parse(genMatch.Groups[1].Value);
            var model = int.Parse(modelMatch.Groups[1].Value);

            return db[type, gen, model];
        }

        public int this[string type, int gen, int model]
        {
            get
            {
                if (type.ToLower() == "iphone")
                {
                    if (gen < 3)
                    {
                        return 163;
                    }

                    // 6 Plus, 6s Plus, 7 Plus
                    if (gen == 7 && model == 1 ||
                        gen == 8 && model == 2 ||
                        gen == 9 && (model == 2 || model == 4) ||
                        gen == 10 && (model == 2 || model == 5))
                    {
                        return 401;
                    }

                    // X, XS Max, 11 Pro (Max), 12 Pro Max
                    if (gen == 10 && (model == 3 || model == 6) ||
                       gen == 11 && (model == 1 || model == 6) ||
                       gen == 12 && (model == 3 || model == 5) ||
                       gen == 13 && model == 4)
                    {
                        return 458;
                    }

                    // 12 Mini
                    if (gen == 13 && model == 1)
                    {
                        return 476;
                    }

                    // 12 (Pro)
                    if (gen == 13 && (model == 2 || model == 3))
                    {
                        return 460;
                    }


                    // Most iPhones are 326
                    return 326;
                }
                else if (type.ToLower() == "ipad")
                {
                    // 1, 2
                    if (gen < 3 && model < 5)
                    {
                        return 132;
                    }

                    // first-gen mini
                    if (gen == 2 && model > 4)
                    {
                        return 163;
                    }

                    // newer minis
                    if (gen == 4 && model > 3 ||
                       gen == 5 && model < 3 ||
                       gen == 11 && model < 3)
                    {
                        return 326;
                    }

                    // most iPads are 264
                    return 264;
                }
                else if (type.ToLower() == "ipod")
                {
                    if (gen < 4)
                        return 163;

                    return 326;
                }

                throw new KeyNotFoundException("No such device");
            }
        }
    }
}
