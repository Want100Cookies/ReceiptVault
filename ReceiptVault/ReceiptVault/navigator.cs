using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace ReceiptVault
{
    public class navigator
    {
        private Frame frame;
        public navigator(Frame frame)
        {
            this.frame = frame;
        }

        public void homeClicked()
        {
            this.frame.Navigate(typeof(newEntryScreen));
        }

        public void spendingsClicked()
        {
            //this.frame.Navigate(typeof(spendings));

        }

        public void VATClicked()
        {
          //  this.frame.Navigate(typeof(vat));
        }


    }
}
