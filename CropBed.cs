using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalYearProject
{
    public class CropBed
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int Position { get; set; }
        public string CurrentCrop { get; set; }
    }
}
