using System.Collections.Generic;

namespace MNSuperadmin.Models
{
    public class MenuInfo
    {
        public int ID { get; set; }
        public string Name { get; set; }

        public string Description { get; set; }

        public string Mode { get; set; }

        public string ClientCode { get; set; }

        public List<MenuItem> MenuItems { get; set; }

        public int? MenuID { get; set; }

        public IList<MenuInfo> MenuInfos { get; set; }

        public MenuInfo()
        {
            MenuInfos = new List<MenuInfo>();
        }
    }
}