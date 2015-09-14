using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Serialization;
using Contract;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Options;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace StatisticClient
{
    class Program
    {
        private static MongoCollection<StressResult> _collection;
        private static MongoCollection<StressResult> Collection
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                if (_collection == null)
                {
                    BsonSerializer.RegisterSerializer(typeof(DateTime), new DateTimeSerializer(DateTimeSerializationOptions.LocalInstance));
                    var client = new MongoClient("mongodb://" + Properties.Settings.Default.MongoDbHost);
                    var db = client.GetServer().GetDatabase("StressTest");
                    _collection = db.GetCollection<StressResult>(typeof(StressResult).Name);
                }
                return _collection;
            }
        }

        static void Main()
        {
            using (var writer = new StreamWriter("data.xml"))
            {
                var serializer = new XmlSerializer(typeof(StressResult[]));

                //var enume = Collection.FindAll().GetEnumerator();
                //for (var i = 0L; enume.MoveNext(); i++)
                //{
                //    var entity = enume.Current;
                //    serializer.Serialize(writer, entity);

                //    Console.WriteLine(i);
                //}

                serializer.Serialize(writer, Collection.AsQueryable().ToArray());
            }
        }
    }
}
