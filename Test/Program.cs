using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using com.caijunxiong.dal;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("ha ha");
            var  redis =new RedisHelper(1);
            redis.StringSet("test","ha ha");
            var data = redis.StringGet("test");
            //            var data = RedisHelper.ConnectionString;
            Console.WriteLine(data);
            Console.ReadLine();
        }
    }
}
