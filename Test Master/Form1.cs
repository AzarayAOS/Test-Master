using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace Test_Master
{
    public partial class Form1 : Form
    {
        string filetext;    // расшифрованная строка   
        bool[] currect;     // на какой вопрос ответили, а на какой нет
        bool[] tORf;        // есть ли производный ответ
        string[] que;       // строка ответов
        int nque;           // номер текущего вопроса
        int n;              // количество вопросов
        int index;          // положение в файле
        Regex reg;          // конструкт для поиска шаблонов в строке файлов
        Match match;        // контейнер для хранения регулярных выражений
        string[][] namber;  // набор, тут хранятся и вопросы и варианты ответов
                            // сам вопрос находиться под индексом 0,
                            // а ответы далее


        // алгоритм шифрования
        static byte[] EncryptStringToBytes(string plainText, byte[] Key, byte[] IV)
        {
            // Проверка аргументов
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");
            byte[] encrypted;
            // Создание RijndaelManaged объекта
            // с указанным ключом и IV.
            using (RijndaelManaged rijAlg = new RijndaelManaged())
            {
                rijAlg.Key = Key;
                rijAlg.IV = IV;

                // Создание декоратора для выполнения поток преобразования.
                ICryptoTransform encryptor = rijAlg.CreateEncryptor(rijAlg.Key, rijAlg.IV);

                // Создание потоков, используемых для шифрования.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {

                            //Запишите все данные в поток.
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }


            // Возврат к зашифрованным байты из потока памяти.
            return encrypted;

        }


        // Алгоритм дешифровки кода
        static string DecryptStringFromBytes(byte[] cipherText, byte[] Key, byte[] IV)
        {
            // Проверка аргументов.
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");

            // Объявляем строку, используемую для хранения
            // расшифрованного текста.
            string plaintext = null;

            // Создание RijndaelManaged объекта
            // с указанным ключом и IV.
            using (RijndaelManaged rijAlg = new RijndaelManaged())
            {
                rijAlg.Key = Key;
                rijAlg.IV = IV;

                // Создание декоратора для выполнения поток преобразования.
                ICryptoTransform decryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV);

                // Создание потоков, используемых для дешифрования.
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {

                            // Чтение зашифрованных байтов из дешифрованного потока
                            // и помещение их в строку.
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }

            }

            return plaintext;
        }


        public Form1()
        {
            InitializeComponent();
            // устанавливаем начальный размер окна
            // а так же начальную видимость элементов
            this.Size = new Size(300, 300);
            progressBar1.Visible = false;
            label1.Visible = false;
            richTextBox1.Visible = false;
            button1.Visible = false;
            button2.Visible = false;

        }

        private void загрузитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // задаем положение элементов
            this.progressBar1.Location = new Point(12, 427);
            this.label1.Location = new Point(335, 30);
            this.richTextBox1.Location = new Point(135, 45);
            this.button1.Location = new Point(200, 375);
            this.button2.Location=new Point(400, 375);


            // задаем размер элементов
            this.Size = new Size(700, 500);
            this.progressBar1.Size = new Size(660, 30);
            this.label1.Size = new Size(47, 13);
            this.richTextBox1.Size = new Size(500, 70);
            this.button1.Size = new Size(100, 47);
            this.button2.Size = new Size(100, 47);

            // отображаем элементы
            progressBar1.Visible = true;
            label1.Visible = true;
            richTextBox1.Visible = true;
            button1.Visible = true;
            button2.Visible = true;

            
            // как ключь используються простые числа
            byte[] key = ASCIIEncoding.ASCII.GetBytes("1212345678912443"); 
            byte[] iV = ASCIIEncoding.ASCII.GetBytes("1212345678912469"); 
            // создаем объект шифровки-дешифровки
            //RijndaelManaged myRijndael = new RijndaelManaged();

            //myRijndael.Key = key;
            //myRijndael.IV = iV;
            openFileDialog1.ShowDialog();

            // считываем набор байтов с файла
            byte[] newtext=File.ReadAllBytes(openFileDialog1.FileName);

            // расшифровываем набор байтов
            filetext = DecryptStringFromBytes(newtext,key, iV);



            // Документация в коде. Тут я пропишу все внутристроковые команды
            // по которым программа разделяет внутри строки, где какая информация
            // чтоб могла правильно интерпретировать заполненную внутри информацию
            // <begin-x-a-t> -- начало очередного набора, где x -- номер набора, 
            //a -- количество активных ответов, 
            //t(t\f)-- есть ли произвольный ответ
            // <bend-x> -- конец набора, где x -- номер набора
            // <que> -- вопрос
            //<qend> -- конец вопроса
            // <a-x> -- активный ответ, где x --  номер ответа
            //<aend-x>
            //<t> -- произвольный ответ
            //<tend>
            //<kol> -- количество вопросов
            // <kend> 

            reg = new Regex(@"<kol>\d+<kend>");
            match = reg.Match(filetext);
            reg = new Regex(@"\d+");
            match = reg.Match(match.Value);
            n = int.Parse(match.Value);

            // выделяем память под переменные контроля
            currect = new bool[n];
            que = new string[n];
            namber = new string[n][];
            tORf = new bool[n];

            nque = 0;

            // отмечаем, что ни на один вопрос ещё не был дан ответ
            for (int i = 0; i < n; i++) currect[i] = false;

            // производим заполнение таблицы вопрос--ответы

            reg = new Regex(@"<begin-\d-\a-t|f>\w+<bend-\d>");
            MatchCollection matchColl=reg.Matches(filetext);

           for(int i=0;i<n;i++)
            {
                string s;
                // вычисляем информацию из тега оформления набора
                // вычисляем количество ответов
                match = Regex.Match(matchColl[i].Value, @"<begin-\d-\a-t|f>");
                match = Regex.Match(match.Value, @"\d");
                match = match.NextMatch();
                int coll =int.Parse (match.Value);
                // вычисляем есть ли произвольный ответ
                match = Regex.Match(matchColl[i].Value, @"<begin-\d-\a-t|f>");
                match = Regex.Match(match.Value, @"t|f");
                tORf[i] = match.Value == "t";

                // выделяем память под коллекцию вопрос--ответы
                namber[i] = new string[coll];

                // выделяем из набора вопрос
                match = Regex.Match(matchColl[i].Value, @"<que>\w<qend>");
                namber[i][0] = match.Value;

                // выделяем из набора ответы
                MatchCollection matchstring = Regex.Matches(matchColl[i].Value, @"<a-\d>\w<aend-\d>");
                // заполняем таблицу вариантами ответов
                for(int j=0;j<coll;j++)
                {
                    namber[i][j] = matchstring[j].Value;
                }

                
            }

            //MatchCollection matches = Regex.Matches(sentence, pattern);
            //for (int ctr = 0; ctr < matches.Count; ctr++)
            //{
            //    Console.WriteLine(matches[ctr].Value);
            //}

            // Документация в коде. Тут я пропишу все внутристроковые команды
            // по которым программа разделяет внутри строки, где какая информация
            // чтоб могла правильно интерпретировать заполненную внутри информацию
            // <begin-x-a-t> -- начало очередного набора, где x -- номер набора, 
            //a -- количество активных ответов, 
            //t(t\f)-- есть ли произвольный ответ
            // <bend-x> -- конец набора, где x -- номер набора
            // <que> -- вопрос
            //<qend> -- конец вопроса
            // <a-x> -- активный ответ, где x --  номер ответа
            //<aend-x>
            //<t> -- произвольный ответ
            //<tend>
            //<kol> -- количество вопросов
            // <kend> 


        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {

        }
    }
}
