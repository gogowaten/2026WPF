using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _20260224
{
    public static class BoundsCalculator
    {
        public static (double Width, double Height) GetTotalSize(IEnumerable<Item> items)
        {
            if (!items.Any()) { return (0, 0); }

            // 各アイテムの「右端」と「下端」の最大値を探す
            double maxX = items.Max(i => i.Right);
            double maxY = items.Max(i => i.Bottom);

            return (maxX, maxY);

            //var itemList = items.ToList();
            //if (!itemList.Any()) return (0, 0);

            //// 各アイテムの Right (X + Width) と Bottom (Y + Height) の最大値
            //double maxX = itemList.Max(i => i.Right);
            //double maxY = itemList.Max(i => i.Bottom);

            //return (maxX, maxY);
        }
    }
}
