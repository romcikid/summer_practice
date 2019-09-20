//  Подключаем необходимые библиотеки.
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;
using MongoDB.Bson;
using MongoDB.Driver;
using Emgu.CV.CvEnum;

namespace L1ttleBrother
{
    public partial class Form1 : Form
    {
        //  Объявляем переменные.
        private Image<Gray, byte> next_frame;
        private VideoCapture capture;
        private Bitmap bitmap;
        private FaceRecognition faceRecognition = new FaceRecognition();

        public Form1()
        {
            InitializeComponent();

            if (capture == null)
            {
                capture = new VideoCapture();
            }

            capture.ImageGrabbed += Capture_ImageGrabbed1;
        }

        //  Метод, осуществляющий захват видеопотока с камеры.
        private void Capture_ImageGrabbed1(object sender, EventArgs e)
        { 
             Mat m = new Mat();
             capture.Retrieve(m, 0);
        }

        //  Метод, запускаемый совместно с самим приложением.
        private void Timer1_Tick(object sender, EventArgs e)
        {using (next_frame = capture.QueryFrame().ToImage<Gray, byte>().Resize(320,240, Emgu.CV.CvEnum.Inter.Cubic))
            {
                //  Объявление переменных.
                Image<Gray, byte> imgGray;
                Rectangle[] faces;
                CascadeClassifier classifier_face;
                string face_path;
                string label;
                Employee employee = new Employee();
                System.Collections.Generic.List<Employee> data;
                IMongoCollection<Employee> collection;

                //  Создание классификатора для распознования лиц в видеопотоке. 
                face_path = Path.GetFullPath(@"C:\Users\romav\source\repos\WindowsFormsApp1\WindowsFormsApp1\data\lbpcascade_frontalface.xml");
                classifier_face = new CascadeClassifier(face_path);

                //  Захват видеопотока.
                capture.ImageGrabbed += Capture_ImageGrabbed1;

                //  Создание копии текущего кадра.
                imgGray = next_frame.Clone();

                //  Нормализация копии кадра.
                CvInvoke.EqualizeHist(imgGray, imgGray);

                //  Распознование лиц в кадре. Сохранение координат границ лиц в массив.
                faces = classifier_face.DetectMultiScale(imgGray, 1.1, 4);

                //  Добавление прямоугольника для обозначения лица в кадре.
                foreach (var face in faces)
                {
                    //  Добавление на изображение рамки вокруг лица.
                    next_frame.Draw(face, new Gray(), 2);

                    //  Выделение найденного лица и превращение в отдельный кадр.
                    Face_to_Frame(face);

                    //  Получение предположение о текущем лице.
                    label = faceRecognition.Recognize_me(capture);


                    //  Подключение к базе данных.
                    collection = employee.Connection_to_DB();

                    // Пустой фильтр для прочтения всех документов.
                    data = collection.Find(new BsonDocument()).ToList();

                    //  Просмотр документов в коллекции.
                    foreach (var i in data)

                        //  Если произошло совпадение меток,
                        if (i.Id == Convert.ToInt32(label))
                        {
                            /* то происходит
                             * добавление информации о распознанном лице на изображение;*/

                            CvInvoke.PutText(next_frame,
                                label,
                                new Point(face.Width - 10, face.Height - 10),
                                FontFace.HersheyComplex,
                                1.0,
                                new Gray().MCvScalar);

                            //  запуск обновления информации о местоположении.
                            Update_location(label);
                        }
                }
                //  Передача изображение в pictureBox1.
                pictureBox1.Image = next_frame.Bitmap;

                //  Передача обнаруженного лица в pictureBox2.
                pictureBox2.Image = bitmap;
            }
        }

        //  Метод, осуществляющий преобразование обнаруженного лица в отдельный кадр.
        private void Face_to_Frame(Rectangle face)
        {
            //  Объявление переменных.
            Bitmap bitmap_frame;
            Graphics graphic_from_frame;

            //  Перевод изображение в массив аттрибутов пикселей.
            bitmap_frame = next_frame.Bitmap;

            //  Преобразование изображения в массив аттрибутов пикселей с заданными параметрами.
            bitmap = new Bitmap(face.Width, face.Height);

            //  Создание поверхности для рисования GDI.
            graphic_from_frame = Graphics.FromImage(bitmap);

            //  Получение выделенное обнаруженное лицо.
            graphic_from_frame.DrawImage(bitmap_frame, 0, 0, face, GraphicsUnit.Pixel);
        }

        //  Метод, конвертирующий изображение в строку.
        private byte[] Image_To_String()
        {
            //  Объявление массива для хранения изображения.
            byte[] result;

            //  Запуск потока памяти.
            MemoryStream stream = new MemoryStream();

            //  Передача изображения формата Bitmap в поток.
            bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Bmp);

            //  Преобразование потока в массив.
            result = stream.ToArray();

            return result;
        }

        //  Метод, вызываемый при нажатии на кнопку "Сохранить".
        private void SaveButton_Click(object sender, EventArgs e)
        {
            //  Объявление переменных.
            DialogResult User_Dialog_Box;
            string question;
            IMongoCollection<Employee> collection;
            System.Collections.Generic.List<Employee> data;
            int max_Id = 0;

            //  Создание экземпляра класса Employee.
            Employee new_employee = new Employee
            {
                //  Заполнение аттрибутов класса из введённых данных.
                Name = textBox1.Text,
                Position = textBox2.Text,
                Face = Convert.ToBase64String(Image_To_String()),
                Location = "В здании"
            };

            question = " Уверены, что хотите добавить сотрудника:" + new_employee.Name + " в базу данных?";

            //  Новое диалоговое окно подтверждения сохранения информации.
            User_Dialog_Box = MessageBox.Show(question,  "Сохранение документа в базе данных", MessageBoxButtons.OKCancel);

            /*  В случае положительного ответа пользователя на вопрос на сохранение записи,
             *  происходит добавление документа в коллекцию Employee базы данных Data_for_app.
             */

            if (User_Dialog_Box == DialogResult.OK)
            {
                //  Создание нового экзмепляра класса.
                Employee employee = new Employee();

                //  Получение коллекции Employee.
                collection = employee.Connection_to_DB();
                
                // Пустой фильтр для прочтения всех документов.
                data = collection.Find(new BsonDocument()).ToList();

                //  Заполнение массива ФИО.
                foreach (var i in data)
                {
                    max_Id = employee.Id;
                }
                //  Получение номера следующего сотрудника.
                max_Id++;

                //  Присвоение порядкого номера для нового сотрудника.
                new_employee.Id = max_Id;
            
                //  Добавление нового документа.
                collection.InsertOne(new_employee);

                //  При успешном сохранении документа появится окно сообщения.
                MessageBox.Show("Новый документ успешно добавлен в коллекцию.");
            }
        }

        //  Метод, вызываемый при выборе пункта меню "НачатьТренировкуСистемы".
        private void НачатьТренировкуСистемыToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //  Инициализация тренировки.
            faceRecognition.TrainRecognizer();

            //  Появление сообщения при удачном выполнении тренировки.
            MessageBox.Show("Тренировка системы выполнена успешно!");
        }

        //  Метод, обновления информации о местоположении сотрудника.
        private void Update_location(string label)
        {
            //  Объявление переменных.
            int id;
            IMongoCollection<Employee> collection;
            System.Collections.Generic.List<Employee> data;

            //  Создание экземплра класса Employee.
            Employee employee = new Employee();

            //  Подключение к базе данных.
            collection = employee.Connection_to_DB();

            // Пустой фильтр для прочтения всех документов.
            data = collection.Find(new BsonDocument()).ToList();

            //  Получение Id номера распознанного сотрудника.
            id = Convert.ToInt32(label);

            /*  Если распознанный сотрудник не находился до этого
             *  в цеху №1, то происходит обновление информации о
             *  его местоположении.*/

            foreach (var i in data)
            {
                if (employee.Id == id)
                    if (employee.Location != "цех №1")

                        //  Обновление информации о местоположении сотрудника.
                        employee.In_the_room(id);
            }
        }
    }
}
