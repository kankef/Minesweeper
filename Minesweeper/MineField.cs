using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Minesweeper
{
    class MineField
    {
        internal enum ClickStatus
        {
            Unclicked,
            Flag,
            Clicked
        };

        public ClickStatus Status;
        public bool isMine;
        public Button button;
        public int neighbourMines;

        public MineField()
        {
            Status = ClickStatus.Unclicked;
            isMine = false;
            neighbourMines = 0;
        }
    }
}
