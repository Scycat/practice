using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace ConsoleApplication2
{
    class Program
    {
        public static double Gen(double mat) //генератор случайной величины, распределенной по экспоненциальному закону
        {
            double x = 0;
            var rnd = new Random();
            while (x == 0) x = -mat * (Math.Log(rnd.NextDouble()));
            return x;
        }
        public static bool ifnotnull(double[] arr)
        {
            bool ans = false;
            foreach (double x in arr)
                if (x != 0) ans = true; //значит, очередь не пуста
            return ans;
        }
        public static bool ifnotfull(double[] arr)
        {
            bool ans = false;
            foreach (double x in arr)
                if (x == 0)
                {
                    ans = true; //значит, в очереди есть свободное место
                    break;
                }
            return ans;
        }
        public static int numlast(double[] arr)
        {
            int ans = arr.Length;
            for (int i = 0; i < arr.Length; i++)
                if (arr[i] == 0)
                {
                    ans = i; //возврат номера последнего свободного места в очереди
                    break;
                }
            return ans;
        }
        static void Main(string[] args)
        {
            double timer = 0.08; //минимальная единица времени
            double timewait1sum = 0;
            int timewait1kol = 0;
            double timewait2sum = 0;
            int timewait2kol = 0;
            double finaltimewait1 = 0;
            double finaltimewait2 = 0;
            //переменные для определения среднего времени ожидания для разных накопителей
            double buf1m = 0, buf2m = 0; //расчет средней длины очереди
            double buf1ms = 0, buf2ms = 0;
            int kol = 0;
            int buf1size = 280, buf2size = 180; //размеры очередей
            int buf1 = 0, buf2 = 0;
            double[] arr1 = new double[buf1size]; //массивы с размерами пакетов (длительностью обслуживания заявок)
            double[] arr2 = new double[buf2size];
            double servtime1 = 0.2, servtime2 = 0.71; //среднее время обслуживания
            double intensity1 = 4.1, intensity2 = 2.2; //средняя интенсивность
            double kpd = 0;
            double load = 0;
            //гибкое переключение на другую очередь после обработки определенного кол-ва заявок, зависит от величины [Количество_недавно_обслуженных_заявок*Их_вес(вес равен длительности обслуживания)]
            double SWITCHINGFINAL1 = 40; // переключение после обработки стольких заявок 1й очереди
            double SWITCHINGFINAL2 = 60; // переключение после обработки стольких заявок 2й очереди
            double SWITCHINGPROGRESS = 0;
            //расширим эту переменную для переключения по величине, зависящей от конкретной очереди
            double SWITCHING = 1;
            //обнуление массивов
            for (int i = 0; i < buf1size; i++)
            {
                arr1[i] = 0;
            }
            for (int i = 0; i < buf2size; i++)
            {
                arr2[i] = 0;
            }
            //
            //Главный цикл работы системы:
            //
            for (double i = timer; i < 100000; i += timer)
            {
                //добавляем новые заявки в очередь
                kol = Convert.ToInt32(Gen(timer * intensity1));
                for (int j = 0; j < kol; j++)
                {
                    if (ifnotfull(arr1))
                    {
                        arr1[numlast(arr1)] = Gen(servtime1);
                        timewait1kol++;
                    }
                }
                kol = Convert.ToInt32(Gen(timer * intensity2));
                for (int j = 0; j < kol; j++)
                {
                    if (ifnotfull(arr2))
                    {
                        arr2[numlast(arr2)] = Gen(servtime2);
                        timewait2kol++;
                    }
                }
                //закончили добавлять новые заявки, теперь обслуживаем заявки, проверка переключения

                if ((SWITCHINGPROGRESS >= SWITCHINGFINAL1) && (SWITCHING == 1))
                {
                    SWITCHING = 2;

                    SWITCHINGPROGRESS = 0; //сбросили прогресс, переключились на очередь 2
                }

                if ((SWITCHINGPROGRESS >= SWITCHINGFINAL2) && (SWITCHING == 2))
                {
                    SWITCHING = 1;

                    SWITCHINGPROGRESS = 0; //сбросили прогресс, переключились на очередь 1
                }

                //обслуживание заявок
                if (SWITCHING == 1)
                {
                    if ((arr1[0] > 0))
                    {
                        arr1[0] -= servtime1;
                        SWITCHINGPROGRESS += servtime1;
                        timewait1sum += servtime1;//прирост времени ожидания
                        if ((arr2[0] > 0))
                        {
                            timewait2sum += servtime1; //прирост времени ожидания
                        }
                    }
                    else SWITCHING = 2; //если заявок в первой очереди нет, идем во вторую
                }

                if (SWITCHING == 2)
                {
                    if ((arr2[0] > 0))
                    {
                        arr2[0] -= servtime2;
                        SWITCHINGPROGRESS += servtime2;
                        timewait2sum += servtime2;//прирост времени ожидания
                        if ((arr1[0] > 0))
                        {
                            timewait1sum += servtime2;//прирост времени ожидания
                        }
                    }
                    else SWITCHING = 1; //если заявок во второй очереди нет, идем в первую
                }

                //закончили обслуживать заявки, теперь сдвигаем массив, чтобы на 1м месте не было "пустого места", а была заявка

                if ((arr1[0] <= 0))
                {
                    for (int j = 0; j < buf1size - 1; j++)
                    {
                        arr1[j] = arr1[j + 1];
                    }
                    arr1[buf1size - 1] = 0;
                }

                if ((arr2[0] <= 0))
                {
                    for (int j = 0; j < buf2size - 1; j++)
                    {
                        arr2[j] = arr2[j + 1];
                    }
                    arr2[buf2size - 1] = 0;
                }

                //подготавливаем информацию для вывода
                buf1 = numlast(arr1);
                buf2 = numlast(arr2);
                buf1ms += buf1;
                buf2ms += buf2;
                buf1m = buf1ms / (i / timer);
                buf2m = buf2ms / (i / timer);
                if ((timewait1kol != 0) && (timewait2kol != 0))
                {
                    finaltimewait1 = timewait1sum / timewait1kol;
                    finaltimewait2 = timewait2sum / timewait2kol;
                }
                Console.Clear();
                Console.WriteLine("В настоящее время обрабатываю заявки из очереди: " + SWITCHING + "\t");
                Console.WriteLine("Первая очередь: " + buf1 + "/" + buf1size + ". Вторая очередь: " + buf2 + "/" + buf2size + "\t");
                Console.WriteLine("Средняя первая оч.: " + buf1m + ". Средняя вторая оч.: " + buf2m + "\t");
                Console.WriteLine("Среднее время ожидания в 1й оч.: " + finaltimewait1 + ". Во 2й оч.: " + finaltimewait2 + "\t");
                Console.WriteLine("Средн. время обсл. в 1й очереди: " + servtime1 + ". Во 2й очереди: " + servtime2 + "\t");
                Console.WriteLine("Средн. интенсивн. заявок 1й оч.: " + intensity1 + ". Во 2й очереди: " + intensity2 + "\t");
                Console.WriteLine("Суммарный размер обработ. заявок для переключения в 1ю/2ю очередь: " + SWITCHINGFINAL2 + "/" + SWITCHINGFINAL1 + "\t");
                Thread.Sleep(10); //для нормальной отрисовки данных
            }
            Console.ReadLine();
        }
    }
}