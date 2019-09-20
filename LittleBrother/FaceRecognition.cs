using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Face;
using Emgu.CV.Structure;
using MongoDB.Bson;

namespace L1ttleBrother
{
    class FaceRecognition
    {
        private readonly string recognizerFilePath = "file.yml";
        FaceRecognizer faceRecognizer = new EigenFaceRecognizer(80, 95);
        Employee employee = new Employee();

        //  Метод, выполняющий тренировку распознователя лиц.
        public void TrainRecognizer()
        {
            //  Объявление и инициализация массивов для изображений:
            byte[][] all_faces = employee.Get_faces();
            Mat[] face_images = new Mat[all_faces.Length];

            //  и меток:
            int[] face_labels;

            //  Объявление переменной.
            Image<Gray, byte> face_image;

            if (all_faces != null)
            {
                //  Инициализируем массив меток.
                face_labels = employee.Get_Ids();

                //  Заполнение массива изображений.
                for (int i = 0; i < all_faces.Length; i++)
                {
                    //  Открытие потока памяти.
                    Stream stream = new MemoryStream();

                    //  Запись в поток памяти изображения в формате массива байтов.
                    stream.Write(all_faces[i], 0, all_faces[i].Length);

                    //  Формирофание массива аттрибутов пикселей изображения из массива байтов.
                    face_image = new Image<Gray, byte>(new Bitmap(stream)).Resize(100, 100, Inter.Cubic);

                    //  Преобразование массива аттрибутов пикселей в матрицу.
                    face_images[i] = face_image.Mat;
                }

                //  Тренировка распознователя лиц.
                faceRecognizer.Train(face_images, face_labels);

                //  Сохранение распознователя в файл.
                faceRecognizer.Write(recognizerFilePath);
            }
            else
                MessageBox.Show("База данных пуста!");
        }

        //  Метод, выполняющий распознование лица.
        public string Recognize_me(VideoCapture capture)
        {
            //  Объявляем переменную для хранения результата распозвнования лица.
            FaceRecognizer.PredictionResult result;
            string return_label;

            //  Чтение тренировочного файла.
            faceRecognizer.Read(recognizerFilePath);

            //  Предположение о найденном лице.
            result = faceRecognizer.Predict(capture.QueryFrame().ToImage<Gray, byte>().Resize(100, 100, Inter.Cubic));

            //  Преобразуем полученное предположение в строку.
            return_label = result.Label.ToString();

            //  Возвращение результата предсказания.
            return return_label;
        }
    }
}