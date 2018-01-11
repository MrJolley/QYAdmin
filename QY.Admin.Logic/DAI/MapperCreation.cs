using QY.Admin.Logic.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QY.Admin.Logic.DAI
{
    public class MapperCreation
    {
        public void CreateMaps()
        {
            AutoMapper.Mapper.CreateMap<User, User>();
        }
    }
}
