using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryApplication.Domain.DTO
{
    public class ApiResponse
    {
        public int TotalItems { get; set; }
        public List<VolumeItem>? Items { get; set; }
    }
}
