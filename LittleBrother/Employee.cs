using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;

namespace L1ttleBrother
{
    class Employee
    {
        [BsonId]
        public int Id { get; set; }

        public string Name { get; set; }

        public string Position { get; set; }

        public string Face { get; set; }

        public string Location { get; set; }

        /*  Метод, подключающийся к базе данных Data_for_app и предоставляющий
        доступ к коллекции документов Employee. */

        public IMongoCollection<Employee> Connection_to_DB()
        {
            //  Объявляем переменные для работы с базой данных.
            MongoClient client;
            IMongoCollection<Employee> collection;
            string data_base_driver = "mongodb+srv://Changy:222@littlebrother-bp44k.mongodb.net/test?retryWrites=true&w=majority";
            
            //  Подключение к базе данных.
            client = new MongoClient(data_base_driver);

            //  Извлечение коллекции.
            collection = client.GetDatabase("Data_for_app").GetCollection<Employee>("employee");

            return collection;
        }

        /*  Метод, собирающий изображения из всех
         *  документов коллекции в один массив. */

        public byte[][] Get_faces()
        {
            //  Объявление переменных.
            IMongoCollection<Employee> collection;
            long number_of_faces;
            byte[][] faces;
            System.Collections.Generic.List<Employee> data;

            //  Объявление и инициализация счётчика лиц.
            int i = 0;

            //  Создание экземпляра класса Employee.
            Employee employee = new Employee();

            //  Подключение к базе данных.
            collection = employee.Connection_to_DB();

            /*  Получение кол-ва документов в коллекции,
             *  а в дальнейшем кол-ва лиц в массиве. */

            number_of_faces = collection.CountDocuments(new BsonDocument());

            // Пустой фильтр для прочтения всех документов.
            data = collection.Find(new BsonDocument()).ToList();
            
            //  Объявление и инициализация массива для хранения лиц.
            faces = new byte[number_of_faces][];

            //  Заполнение массива лицами в формате строки.
            foreach (var e in data)
            {
                faces[i] = Convert.FromBase64String(e.Face);

                //  Увеличение счётчика на единицу.
                i++;
            }
            
            //  Возвращение массива лиц.
            return faces;
        }

        /*  Метод, собирающий Id номера сотрудников из всех
         *  документов коллекции в один массив. */

        public int[] Get_Ids()
        {
            //  Объявление переменных.
            IMongoCollection<Employee> collection;
            long number_of_ids;
            int[] ids;
            System.Collections.Generic.List<Employee> data;

            //  Объявление и инициализация счётчика лиц.
            int i = 0;

            //  Создание экземпляра класса Employee.
            Employee employee = new Employee();

            //  Подключение к базе данных.
            collection = employee.Connection_to_DB();

            /*  Получение кол-ва документов в коллекции,
             *  а в дальнейшем кол-ва лиц в массиве. */

            number_of_ids = collection.CountDocuments(new BsonDocument());


            // Пустой фильтр для прочтения всех документов.
            data = collection.Find(new BsonDocument()).ToList();

            /*  Инициализация массива для хранения
             *  идентификационных номеров.*/

            ids = new int[number_of_ids];

            //  Заполнение массива идентификационных номеров.
            foreach (var e in data)
            {
                ids[i] = employee.Id;

                //  Увеличение счётчика на единицу.
                i++;
            }

            //  Возвращение массива лиц.
            return ids;
        }

        /*  Метод, осуществляющий обновление информации
         *  о местоположении сотрудника при обнаружении.*/

        public void In_the_room(int label)
        {
            //  Объявление переменных.
            IMongoCollection<Employee> collection;
            FilterDefinition<Employee> filter;
            UpdateDefinition<Employee> update;
            UpdateResult result;

            //  Создание экзмепляра класса.
            Employee employee = new Employee();

            //  Подключение к базе данных.
            collection = employee.Connection_to_DB();

            //  Поиск соответствующего документа в коллекции.
            filter = Builders<Employee>.Filter.Eq("Id", label);

            //  Введение новых данных о местоположении.
            update = Builders<Employee>.Update.Set("Location", "цех №1");

            //  Обновление информации о местоположении.
            result = collection.UpdateOne(filter, update);
        }
    }
}
