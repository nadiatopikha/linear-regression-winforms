using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace ai2._1
{
    public partial class Form1 : Form
    {
        private readonly List<List<double>> numbersLists = new List<List<double>>();
        private List<double> epsList = new List<double>();
        private List<double> xList = new List<double>();
        private List<double> yList = new List<double>();
        private List<double> vecY = new List<double>();

        private double prevA;
        private double prevB;
        private double aParameter;
        private double bParameter;
        private double a;
        private double b;
        private double standardDeviation;

        private int index;
        private int N;
        private int n;

        private static readonly Random random = new Random();

        public Form1()
        {
            InitializeComponent();

            comboBox1.SelectedIndexChanged += comboBox1_SelectedIndexChanged;

            chart1.ChartAreas[0].AxisX.LabelStyle.Format = "0.####";
            chart1.ChartAreas[0].AxisY.LabelStyle.Format = "0.####";

            chart2.ChartAreas[0].AxisX.LabelStyle.Format = "0.####";
            chart2.ChartAreas[0].AxisY.LabelStyle.Format = "0.####";
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (numbersLists.Count == 0)
            {
                MessageBox.Show("Спочатку завантажте або згенеруйте дані.", "Помилка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (comboBox1.SelectedIndex < 0 || comboBox1.SelectedIndex >= numbersLists.Count)
            {
                MessageBox.Show("Некоректно вибраний індекс вектора.", "Помилка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string parametersText = textBox1.Text.Trim();
            index = comboBox1.SelectedIndex;

            if (checkBox1.Checked)
            {
                if (string.IsNullOrWhiteSpace(parametersText))
                {
                    MessageBox.Show("Введіть у поле індекс вектора y.", "Помилка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (!int.TryParse(parametersText, out int yIndex))
                {
                    MessageBox.Show("Індекс вектора y має бути цілим числом.", "Помилка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (yIndex < 0 || yIndex >= numbersLists.Count)
                {
                    MessageBox.Show("Індекс вектора y виходить за межі списку.", "Помилка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                N = numbersLists[index].Count;

                if (numbersLists[yIndex].Count != N)
                {
                    MessageBox.Show("Вектори x та y мають різну довжину.", "Помилка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                label1.Text = $"N = {N}\nМат. спод. x = {Math.Round(numbersLists[index].Average(), 4)}";

                vecY = new List<double>(numbersLists[yIndex]);

                double mean1 = numbersLists[index].Average();
                double mean2 = vecY.Average();

                double covariance = 0.0;
                for (int i = 0; i < N; i++)
                {
                    covariance += (numbersLists[index][i] - mean1) * (vecY[i] - mean2);
                }
                covariance /= N;

                double stdDev1 = 0.0;
                double stdDev2 = 0.0;

                for (int i = 0; i < N; i++)
                {
                    stdDev1 += Math.Pow(numbersLists[index][i] - mean1, 2);
                    stdDev2 += Math.Pow(vecY[i] - mean2, 2);
                }

                stdDev1 = Math.Sqrt(stdDev1 / N);
                stdDev2 = Math.Sqrt(stdDev2 / N);

                if (stdDev1 == 0 || stdDev2 == 0)
                {
                    MessageBox.Show("Неможливо обчислити кореляцію: стандартне відхилення дорівнює 0.", "Помилка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                double correlation = covariance / (stdDev1 * stdDev2);

                prevA = mean1;
                prevB = correlation;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(parametersText))
                {
                    MessageBox.Show("Введіть оцінки параметрів a та b, а також індекс вектора y через пробіл.", "Помилка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string[] parametersArray = parametersText
                    .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (parametersArray.Length != 3)
                {
                    MessageBox.Show("Потрібно ввести: a b yIndex", "Помилка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (!double.TryParse(parametersArray[0], NumberStyles.Any, CultureInfo.CurrentCulture, out prevA) &&
                    !double.TryParse(parametersArray[0], NumberStyles.Any, CultureInfo.InvariantCulture, out prevA))
                {
                    MessageBox.Show("Некоректне значення параметра a.", "Помилка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (!double.TryParse(parametersArray[1], NumberStyles.Any, CultureInfo.CurrentCulture, out prevB) &&
                    !double.TryParse(parametersArray[1], NumberStyles.Any, CultureInfo.InvariantCulture, out prevB))
                {
                    MessageBox.Show("Некоректне значення параметра b.", "Помилка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (!int.TryParse(parametersArray[2], out int yIndex))
                {
                    MessageBox.Show("Індекс вектора y має бути цілим числом.", "Помилка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (yIndex < 0 || yIndex >= numbersLists.Count)
                {
                    MessageBox.Show("Індекс вектора y виходить за межі списку.", "Помилка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                N = numbersLists[index].Count;

                if (numbersLists[yIndex].Count != N)
                {
                    MessageBox.Show("Вектори x та y мають різну довжину.", "Помилка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                label1.Text = $"N = {N}\nМат. спод. x = {Math.Round(numbersLists[index].Average(), 4)}";
                vecY = new List<double>(numbersLists[yIndex]);
            }

            GradientDescentMethod();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            chart2.Series[0].Points.Clear();
            chart2.Series[1].Points.Clear();

            string textBoxText = textBox2.Text.Trim();
            List<double> parametersList = ParseAndValidateInput(textBoxText);

            if (parametersList == null)
            {
                MessageBox.Show("Помилка розбору параметрів. Введено некоректні дані.", "Помилка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            aParameter = parametersList[0];
            bParameter = parametersList[1];
            a = parametersList[2];
            b = parametersList[3];
            n = (int)parametersList[4];
            standardDeviation = parametersList[5];

            if (n <= 0)
            {
                MessageBox.Show("Кількість елементів n має бути більшою за 0.", "Помилка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (a >= b)
            {
                MessageBox.Show("Для генерації x потрібно, щоб xmin < xmax.", "Помилка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            epsList = GenerateEps(n, -3 * standardDeviation, 3 * standardDeviation);
            xList = GenerateRandomNumbers(n, a, b);
            yList = CalculateYList(aParameter, bParameter);

            numbersLists.Add(new List<double>(xList));
            numbersLists.Add(new List<double>(yList));

            int xIndex = numbersLists.Count - 2;
            int yIndex = numbersLists.Count - 1;

            comboBox1.Items.Add($"{xIndex} згенерований x");
            comboBox1.Items.Add($"{yIndex} згенерований y");

            double minX = xList.Min();
            double maxX = xList.Max();

            chart2.Series[1].Points.AddXY(minX, aParameter + bParameter * minX);
            chart2.Series[1].Points.AddXY(maxX, aParameter + bParameter * maxX);

            for (int j = 0; j < xList.Count; j++)
            {
                chart2.Series[0].Points.AddXY(xList[j], yList[j]);
            }
        }

        private static List<double> GenerateRandomNumbers(int count, double minValue, double maxValue)
        {
            List<double> randomNumbersList = new List<double>();

            for (int i = 0; i < count; i++)
            {
                double randomNumber = random.NextDouble() * (maxValue - minValue) + minValue;
                randomNumbersList.Add(randomNumber);
            }

            return randomNumbersList;
        }

        private static List<double> GenerateEps(int count, double minValue, double maxValue)
        {
            List<double> randomNumbersList = new List<double>();

            for (int i = 0; i < count; i++)
            {
                double sum = 0;

                for (int j = 0; j < 5; j++)
                {
                    double randomNumber = random.NextDouble() * (maxValue - minValue) + minValue;
                    sum += randomNumber;
                }

                double average = sum / 5.0;
                randomNumbersList.Add(average);
            }

            return randomNumbersList;
        }

        private List<double> CalculateYList(double aParameter, double bParameter)
        {
            List<double> result = new List<double>();

            for (int i = 0; i < xList.Count; i++)
            {
                double y = aParameter + bParameter * xList[i] + epsList[i];
                result.Add(y);
            }

            return result;
        }

        public (double aParameter, double bParameter, int xmin, int xmax, int n, double standardDeviation) GenerateParameters()
        {
            double generatedAParameter = random.Next(-1000, 1000);
            double generatedBParameter = random.Next(-20, 20);
            int xmin = random.Next(0, 100);
            int xmax = xmin + random.Next(5, 30);
            int generatedN = random.Next(100, 800);
            double generatedStandardDeviation = random.Next(40, 60) + random.NextDouble();

            return (generatedAParameter, generatedBParameter, xmin, xmax, generatedN, generatedStandardDeviation);
        }

        private static List<double> ParseAndValidateInput(string inputText)
        {
            string[] inputArray = inputText
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (inputArray.Length != 6)
            {
                return null;
            }

            List<double> parametersList = new List<double>();

            foreach (string item in inputArray)
            {
                bool parsed =
                    double.TryParse(item, NumberStyles.Any, CultureInfo.CurrentCulture, out double result) ||
                    double.TryParse(item, NumberStyles.Any, CultureInfo.InvariantCulture, out result);

                if (!parsed)
                {
                    return null;
                }

                parametersList.Add(result);
            }

            return parametersList;
        }

        private void GradientDescentMethod()
        {
            if (numbersLists.Count == 0 || vecY == null || vecY.Count == 0)
            {
                MessageBox.Show("Немає даних для обчислення.", "Помилка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            double h = 0.0001;
            double eps = 0.001;
            int maxIter = 500000;

            double currentA = prevA;
            double currentB = prevB;

            double previousS = CalculateLoss(currentA, currentB);
            int iter = 0;

            while (iter < maxIter)
            {
                iter++;

                double sumA = 0.0;
                double sumB = 0.0;

                for (int i = 0; i < N; i++)
                {
                    double error = vecY[i] - currentA - currentB * numbersLists[index][i];
                    sumA += -error;
                    sumB += -error * numbersLists[index][i];
                }

                double newA = currentA - h * sumA / N;
                double newB = currentB - h * sumB / N;

                double currentS = CalculateLoss(newA, newB);

                if (Math.Abs(previousS - currentS) < eps)
                {
                    currentA = newA;
                    currentB = newB;
                    break;
                }

                currentA = newA;
                currentB = newB;
                previousS = currentS;
            }

            prevA = currentA;
            prevB = currentB;

            double minX = numbersLists[index].Min();
            double maxX = numbersLists[index].Max();

            chart1.Series[1].Points.Clear();
            chart1.Series[1].Points.AddXY(minX, currentA + currentB * minX);
            chart1.Series[1].Points.AddXY(maxX, currentA + currentB * maxX);

            chart1.Series[0].Points.Clear();
            for (int i = 0; i < numbersLists[index].Count; i++)
            {
                chart1.Series[0].Points.AddXY(numbersLists[index][i], vecY[i]);
            }
        }

        private double CalculateLoss(double aValue, double bValue)
        {
            double s = 0.0;

            for (int i = 0; i < N; i++)
            {
                s += Math.Pow(vecY[i] - aValue - bValue * numbersLists[index][i], 2);
            }

            return s;
        }

        private string OpenFile()
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

                DialogResult result = openFileDialog.ShowDialog();

                if (result == DialogResult.OK)
                {
                    return openFileDialog.FileName;
                }

                return null;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string filePath = OpenFile();

            if (string.IsNullOrWhiteSpace(filePath))
            {
                return;
            }

            string[] readText = File.ReadAllLines(filePath);

            if (readText.Length == 0)
            {
                MessageBox.Show("Файл порожній.", "Помилка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string[] firstRow = Regex.Split(readText[0], @"\s+")
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToArray();

            int columnCount = firstRow.Length;

            if (columnCount == 0)
            {
                MessageBox.Show("Не вдалося визначити кількість стовпців.", "Помилка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            List<List<double>> loadedColumns = new List<List<double>>();
            for (int i = 0; i < columnCount; i++)
            {
                loadedColumns.Add(new List<double>());
            }

            foreach (string line in readText)
            {
                string[] values = Regex.Split(line, @"\s+")
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .ToArray();

                if (values.Length == 0)
                {
                    continue;
                }

                for (int i = 0; i < columnCount; i++)
                {
                    string value = i < values.Length ? values[i] : "0";

                    bool parsed =
                        double.TryParse(value, NumberStyles.Any, CultureInfo.CurrentCulture, out double doubleValue) ||
                        double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out doubleValue);

                    if (!parsed)
                    {
                        MessageBox.Show($"Некоректне число у файлі: \"{value}\"", "Помилка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    loadedColumns[i].Add(doubleValue);
                }
            }

            foreach (var column in loadedColumns)
            {
                numbersLists.Add(column);
                comboBox1.Items.Add(numbersLists.Count - 1);
            }

            MessageBox.Show("Дані успішно завантажено.", "Готово",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}