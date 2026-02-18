using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
// 1. モデルクラス（階層データ用）

namespace _20260218_HeaderItemControl2
{
    public class Node
    {
        public string Name { get; set; } = string.Empty;
        public ObservableCollection<Node> Children { get; set; } = [];
    }
}
