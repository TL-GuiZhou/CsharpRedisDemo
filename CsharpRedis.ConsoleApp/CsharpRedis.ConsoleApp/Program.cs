using System;
using System.Collections.Generic;
using ServiceStack.Redis;

namespace CsharpRedis.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var manager = new CsharpRedisHelper("127.0.0.1:6379", 100, 100);

            manager.UsingClient((client) =>
            {
                Console.WriteLine(string.Join(",", client.GetAllKeys()));
            });


            var data = new List<Entity>();

            for (var i = 0; i < 10; i++)
            {
                data.Add(new Entity { id = Guid.NewGuid().ToString() });
            }

            manager.Add("entity1", data, DateTime.Now.AddDays(1));
            var item=manager.Get<List<Entity>>("entity1");
          
          
            Console.ReadLine();
        }

      
    }


    public class CsharpRedisHelper
    {
        public PooledRedisClientManager RedisClientManager { get; }
        public CsharpRedisHelper(string host,int maxWritePoolSize,int maxReadPoolSize)
        {
            var redisHosts = new List<string>()
            {
                host
            };
            RedisClientManager = new PooledRedisClientManager(redisHosts, redisHosts,new RedisClientManagerConfig()
                {
                    MaxWritePoolSize = maxWritePoolSize,
                    MaxReadPoolSize = maxReadPoolSize,
                    AutoStart = true
                });
        }



        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expires">超时时间</param>
        /// <param name="cover">存在则更新</param>
        public void Add<T>(string key, T value,DateTime expires,bool cover=true)
        {
            using (var r = RedisClientManager.GetClient())
            {
                if (r == null) return;
                if (r.ContainsKey(key) && !cover) throw new ArgumentException("目标已存在");
                r.SendTimeout = 1000;
                r.Set(key, value, expires - DateTime.Now);
            }
        }

        public T Get<T>(string key)
        {
            using (var r = RedisClientManager.GetClient())
            {
                if (r == null) return default(T);
                if (!r.ContainsKey(key)) throw new ArgumentException("目标不存在");
                r.SendTimeout = 1000;
                return r.Get<T>(key);
            }
        }

        public void Remove(string key)
        {
            using (var r = RedisClientManager.GetClient())
            {
                if (r == null) return;
                if (!r.ContainsKey(key)) throw new ArgumentException("目标不存在");
                r.Remove(key);
            }
        }

        public void Update<T>(string key,T t)
        {
            using (var r = RedisClientManager.GetClient())
            {
                if (r == null) return;
                if (!r.ContainsKey(key)) throw new ArgumentException("目标不存在");
                r.Set(key, t);
            }
        }


        public void UsingClient(Action<IRedisClient> action)
        {
            using (var r = RedisClientManager.GetClient())
            {
                action(r);
            }
        }

        public bool ContainsKey(string key)
        {
            var res = false;
            UsingClient((c) => { res=c.ContainsKey(key); });
            return res;
        }
    }

    public class Entity
    {
        public  string id { get; set; }
        public  string name { get; set; }
    }
}
