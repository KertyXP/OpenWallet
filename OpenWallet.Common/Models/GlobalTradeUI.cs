namespace OpenWallet.Common.Models
{
    public class GlobalTradeUI
    {
        public GlobalTrade Trade { get; set; }
        public double Delta { get; set; }
        public bool IsProfitable { get; set; }
        public System.Drawing.Color SellStateBackColor { get; set; }
        public System.Drawing.Color SellStateBackColorSelected { get; set; }
        public System.Drawing.Color ArchiveStateForeColor { get; set; }
        public System.Drawing.Color DeltaColor { get; set; }
        public System.Drawing.Color DeltaColorSelected { get; set; }
      
    }
}