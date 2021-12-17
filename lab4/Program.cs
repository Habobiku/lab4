
using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lab4
{
    class Program
    {
        static void Main()
         {
            Random random = new Random();
            int a = 300;
            int[,] routes = new int[a,a];
            for (int i = 0; i < a; i++)
            {
                for (int j = 0; j < a; j++)
                {
                    if (i == j)
                    {
                        routes[i, j] = 0;
                    }
                    else
                    {
                        routes[i, j] = random.Next(5, 25);
                    }
                   
                }
               
            }
            GeneticAlgo solv = new GeneticAlgo(routes);
            solv.Start();
            
       
            
            
        }

    }



    class GeneticAlgo
    {
      
        private readonly int _countOfPopulation = 100;
        private readonly int _crossingFactor = 70;
        private readonly int _crossingFactorTwoPointFirst = 70;
        private readonly int _crossingFactorTwoPointSecond = 140;
        //private readonly int _capacity = 250;
        private readonly int _mutationProbability = 10;// in range 0 to 100
        private readonly int _iterations = 1000;

        private int[,] _population;
        private int[,] _routes;
        public int[] _path;
        Random rand = new Random();

        public GeneticAlgo(int[,] routes)
        {
            _routes = routes;
        }
       
        public void Start()
        {
            InitPopulation();
            for (int i = 0; i < _iterations; i++)
            {
                int[] changed = BestRandCrossoverTwoPoint();
                _path = MatrixToSet(SetWithMinLength());
                if (changed[0] != -1)
                {
                    SwapMutatuion(changed[0]);
                    _path = MatrixToSet(SetWithMinLength());
                    TwoOptLocalImprovement(changed[0]);
                    _path = MatrixToSet(SetWithMinLength());
                }
                if (changed[1] != -1)
                {
                    SwapMutatuion(changed[1]);
                    _path = MatrixToSet(SetWithMinLength());
                    TwoOptLocalImprovement(changed[1]);
                    _path = MatrixToSet(SetWithMinLength());
                }

                if ((i + 1) % 20 == 0)
                {
                    Console.WriteLine($"iteration: {i + 1}");
                    _path = MatrixToSet(SetWithMinLength());
                    Console.WriteLine("Min distance: " + findPathLength(_path));
                    for (int j = 0; j < _path.Length; j++)
                    {
                        Console.Write(_path[j] + "-");
                    }
                    Console.WriteLine();
                }
            }
        }

        private void InitPopulation()
        {
            int[] first = GreedyAlgo();
            _population = new int[_countOfPopulation, _routes.GetLength(0)];
            AddToMatrix(_population, first, 0);
            for (int i = 1; i < _population.GetLength(0); i++)
            {
                AddToMatrix(_population, Shuffle(first), i);
            }

            /*int size = _routes.GetLength(0);
            _population = new int[_countOfPopulation, _routes.GetLength(0)];
            for (int i = 0; i < _countOfPopulation; i++)
            {
                int[] randomSequence = GenerateRandom(size, size).ToArray();
                for (int j = 0; j < _routes.GetLength(0); j++)
                {
                    _population[i, j] = randomSequence[j];
                }
            }*/
        }
        public static List<int> GenerateRandom(int count, int maxVal)
        {
            Random random = new Random();
            // generate count random values.
            HashSet<int> candidates = new HashSet<int>();
            while (candidates.Count < count)
            {
                // May strike a duplicate.
                candidates.Add(random.Next(0, maxVal));
            }

            // load them in to a list.
            List<int> result = new List<int>();
            result.AddRange(candidates);

            // shuffle the results:
            int i = result.Count;
            while (i > 1)
            {
                i--;
                int k = random.Next(i + 1);
                int value = result[k];
                result[k] = result[i];
                result[i] = value;
            }
            return result;
        }
        private int[] Shuffle(int[] path)
        {
            int i = path.Length;
            while (i > 1)
            {
                i--;
                int k = rand.Next(i + 1);
                int value = path[k];
                path[k] = path[i];
                path[i] = value;
            }
            return path;
        }
        private int[] BestRandCrossoverOnePoint()
        {
            int[] changed = { -1, -1 };
            int minCostKind = SetWithMinLength();
            //Console.WriteLine(minCostKind);
            int random = RandomPopulation(minCostKind);
            int[] first = MatrixToSet(minCostKind);
            int[] second = MatrixToSet(random);
            int[] firstChild = GetChildOnePoint(first, second);
            int[] secondChild = GetChildOnePoint(second, first);
            int firstChildLength = findPathLength(firstChild);
            int secondChildLength = findPathLength(secondChild);
            int worth = SetWithMaxLength();
            if (CanInsert(firstChildLength, worth))
            {
                SetToMatrix(firstChild, worth);
                changed[0] = worth;
            }
            worth = SetWithMaxLength();
            if (CanInsert(secondChildLength, worth))
            {
                SetToMatrix(secondChild, worth);
                changed[1] = worth;
            }
            return changed;
        }
        private int[] GetChildOnePoint(int[] first, int[] second)
        {
            List<int> child = new List<int>();
            List<int> inserted = new List<int>();
            for (int i = 0; i < _crossingFactor; i++)
            {
                child.Add(first[i]);
                inserted.Add(first[i]);
            }
            List<int> differenceList = RemoveAdded(inserted);
            List<int> ddifferenceList = first.Distinct().ToList();
            //differenceList = differenceList.Distinct().ToList();
            //for (int i = 0; i < differenceList.Count; i++)
            //{
            //    Console.Write(differenceList[i] + ",");
            //}
            //for (int i = 0; i < inserted.Count; i++)
            //{
            //    Console.Write(inserted[i] + ",");
            //}

            for (int i = 0; i < differenceList.Count; i++)
            {
                child.Add(differenceList[i]);
            }
            child = child.Distinct().ToList();
            int[] result = child.ToArray();
            return result;
        }

        private int[] BestRandCrossoverTwoPoint()
        {
            int[] changed = { -1, -1 };
            int minCostKind = SetWithMinLength();
            //Console.WriteLine(minCostKind);
            int random = RandomPopulation(minCostKind);
            int[] first = MatrixToSet(minCostKind);
            int[] second = MatrixToSet(random);
            int[] firstChild = GetChildOTwoPoint(first, second);
            int[] secondChild = GetChildOTwoPoint(second, first);
            int firstChildLength = findPathLength(firstChild);
            int secondChildLength = findPathLength(secondChild);
            int worth = SetWithMaxLength();
            if (CanInsert(firstChildLength, worth))
            {
                SetToMatrix(firstChild, worth);
                changed[0] = worth;
            }
            worth = SetWithMaxLength();
            if (CanInsert(secondChildLength, worth))
            {
                SetToMatrix(secondChild, worth);
                changed[1] = worth;
            }
            return changed;
        }
        private int[] GetChildOTwoPoint(int[] first, int[] second)
        {
            int size = _routes.GetLength(0);
            List<int> child = new List<int>();


            List<int> inserted = new List<int>();
            for (int i = 0; i < _crossingFactorTwoPointFirst; i++)
            {
                inserted.Add(first[i]);
            }

            for (int i = _crossingFactorTwoPointSecond; i < size; i++)
            {
                inserted.Add(first[i]);
            }

            List<int> differenceList = second.Except(inserted).ToList();

            for (int i = 0; i < _crossingFactorTwoPointFirst; i++)
            {
                child.Add(first[i]);
            }
            for (int i = 0; i < differenceList.Count; i++)
            {
                child.Add(differenceList[i]);
            }
            for (int i = _crossingFactorTwoPointSecond; i < size; i++)
            {
                child.Add(first[i]);
            }

            child = child.Distinct().ToList();
            int[] result = child.ToArray();
            return result;
        }

        private int[] BestRandCrossoverPMX()
        {
            int[] changed = { -1, -1 };
            int minCostKind = SetWithMinLength();
            //Console.WriteLine(minCostKind);
            int random = RandomPopulation(minCostKind);
            int[] first = MatrixToSet(minCostKind);
            int[] second = MatrixToSet(random);
            int[] firstChild = GetChildPMX(first, second);
            int[] secondChild = GetChildPMX(second, first);
            int firstChildLength = findPathLength(firstChild);
            int secondChildLength = findPathLength(secondChild);
            int worth = SetWithMaxLength();
            if (CanInsert(firstChildLength, worth))
            {
                SetToMatrix(firstChild, worth);
                changed[0] = worth;
            }
            worth = SetWithMaxLength();
            if (CanInsert(secondChildLength, worth))
            {
                SetToMatrix(secondChild, worth);
                changed[1] = worth;
            }
            return changed;
        }
        private int[] GetChildPMX(int[] first, int[] second)
        {
            int size = _routes.GetLength(0);
            int[] child = new int[size];
            for (int i = 0; i < size; i++)
            {
                child[i] = -1;
            }

            List<int> inserted = new List<int>();
            for (int i = _crossingFactorTwoPointFirst; i < _crossingFactorTwoPointSecond; i++)
            {
                child[i] = second[i];
                inserted.Add(second[i]);
            }

            for (int i = 0; i < _crossingFactorTwoPointFirst; i++)
            {
                if (inserted.Contains(first[i]))
                {
                    child[i] = -1;
                }
                else
                {
                    child[i] = first[i];
                    inserted.Add(first[i]);
                }

            }
            for (int i = _crossingFactorTwoPointSecond; i < size; i++)
            {
                if (inserted.Contains(first[i]))
                {
                    child[i] = -1;
                }
                else
                {
                    child[i] = first[i];
                    inserted.Add(first[i]);
                }

            }

            for (int i = 0; i < _crossingFactorTwoPointFirst; i++)
            {
                if (!inserted.Contains(second[i]) && child[i] == -1)
                {
                    child[i] = second[i];
                    inserted.Add(second[i]);
                }

            }
            for (int i = _crossingFactorTwoPointSecond; i < size; i++)
            {
                if (!inserted.Contains(second[i]) && child[i] == -1)
                {
                    child[i] = second[i];
                    inserted.Add(second[i]);
                }

            }

            inserted = inserted.Distinct().ToList();

            int[] threeHundreed = new int[_routes.GetLength(0)];

            for (int k = 0; k < size; k++)
            {
                threeHundreed[k] = k;
            }
            List<int> differenceList = threeHundreed.Except(inserted).ToList();

            int counter = 0;
            for (int i = 0; i < size; i++)
            {
                if (child[i] == -1)
                {
                    child[i] = differenceList[counter];
                    counter++;
                }
            }
            child = child.Distinct().ToList().ToArray();
            int[] result = child.ToArray();
            return result;
        }

        private void SwapMutatuion(int changed)
        {
            int mutation = rand.Next(101);
            int size = _routes.GetLength(0);
            int gen1 = rand.Next(size);
            int gen2 = rand.Next(size);
            do
            {
                gen2 = rand.Next(size);
            } while (gen1 == gen2);

            if (mutation <= _mutationProbability)
            {
                int temp = _population[changed, gen1];
                _population[changed, gen1] = _population[changed, gen2];
                _population[changed, gen2] = temp;
            }
        }
        private void ReverseMutation(int changed)
        {
            int prob = rand.Next(100);
            if (prob < _mutationProbability)
            {
                int[] changedArr = MatrixToSet(changed);
                int first;
                int second;
                do
                {
                    first = rand.Next(_routes.GetLength(0));
                    second = rand.Next(_routes.GetLength(0));
                } while (first == second);
                if (first > second)
                {
                    int tmp = first;
                    first = second;
                    second = tmp;
                }
                int[] result = TwoOptSwap(changedArr, first, second);
                AddToMatrix(_population, result, changed);
            }
        }
        private List<int> RemoveAdded(List<int> inserted)
        {
            List<int> result = new List<int>();
            for (int i = 0; i < _routes.GetLength(0); i++)
            {
                result.Add(i);
            }

            for (int i = 0; i < result.Count; i++)
            {
                for (int j = 0; j < inserted.Count; j++)
                {
                    if (result[i] == inserted[j])
                    {
                        result[i] = -1;
                    }
                }
            }
            for (int i = 0; i < result.Count; i++)
            {
                result.Remove(-1);
            }
            return result;
        }
        private bool CanInsert(int length, int worth)
        {
            if (length <= findPathLength(MatrixToSet(worth)))
            {
                return true;
            }
            return false;
        }
        private void TwoOptLocalImprovement(int changed)
        {
            int[] changedArr = MatrixToSet(changed);
            int bestDistance = findPathLength(changedArr);
            int[] newRoute;
            for (int i = 0; i < _routes.GetLength(0); i++)
            {
                for (int j = i + 1; j < _routes.GetLength(0) - 1; j++)
                {
                    newRoute = TwoOptSwap(changedArr, i, j);
                    int newDistance = findPathLength(newRoute);
                    if (newDistance < bestDistance)
                    {
                        changedArr = newRoute;
                        bestDistance = newDistance;
                    }
                }
            }
            AddToMatrix(_population, changedArr, changed);
        }
        private int[] TwoOptSwap(int[] path, int _i, int _j)
        {
            //int[] newPath = new int[path.Length];
            List<int> newPath = new List<int>();
            List<int> tmp = new List<int>();
            for (int i = 0; i < _i; i++)
            {
                newPath.Add(path[i]);
            }
            for (int i = _i; i <= _j; i++)
            {
                tmp.Add(path[i]);
            }
            tmp.Reverse();
            for (int i = 0; i < tmp.Count; i++)
            {
                newPath.Add(tmp[i]);
            }
            for (int i = _j + 1; i < path.Length; i++)
            {
                newPath.Add(path[i]);
            }
            return newPath.ToArray();
        }
        private void ThreeOptLocalImprovement(int changed)
        {
            int[] changedArr = MatrixToSet(changed);
            int bestDistance = findPathLength(changedArr);
            int size = _routes.GetLength(0);
            for (int i = 1; i < size - 3; ++i)
            {
                for (int j = i + 1; j < size - 2; ++j)
                {
                    for (int k = j + 1; k < size - 1; ++k)
                    {
                        // Perform the 3 way swap and test the length
                        SwapIndexes(changedArr, i, k);
                        SwapIndexes(changedArr, j, k);
                        int newDistance = findPathLength(changedArr);

                        if (newDistance < bestDistance)
                        {
                        }
                        else
                        {
                            SwapIndexes(changedArr, j, k);
                            SwapIndexes(changedArr, i, k);
                        }
                    }
                }
            }
            AddToMatrix(_population, changedArr, changed);
        }
        private void SwapIndexes(int[] changedArr, int i, int k)
        {
            int temp = changedArr[i];
            changedArr[i] = changedArr[k];
            changedArr[k] = temp;
        }
        private int SetWithMinLength()
        {
            int minCost = Int32.MaxValue;
            int vertex = -1;
            for (int i = 0; i < _countOfPopulation; i++)
            {
                //Console.WriteLine(i);
                int tmp = findPathLength(MatrixToSet(i));
                if (minCost > tmp)
                {
                    minCost = tmp;
                    vertex = i;
                }
            }
            return vertex;
        }
        private int SetWithMaxLength()
        {
            int maxCost = 0;
            int vertex = -1;
            for (int i = 0; i < _countOfPopulation; i++)
            {
                int tmp = findPathLength(MatrixToSet(i));
                if (maxCost < tmp)
                {
                    maxCost = tmp;
                    vertex = i;
                }
            }
            return vertex;
        }
        private void SetToMatrix(int[] set, int numOfSet)
        {
            for (int i = 0; i < _routes.GetLength(0); i++)
            {

                _population[numOfSet, i] = set[i];
            }
        }
        private int RandomPopulation(int numOfSet)
        {
            int result;
            do
            {
                result = rand.Next(_countOfPopulation);
            } while (result == numOfSet);
            return result;
        }
        private int[] MatrixToSet(int numOfSet)
        {
            int[] result = new int[_routes.GetLength(0)];
            for (int i = 0; i < _routes.GetLength(0); i++)
            {
                result[i] = _population[numOfSet, i];
            }
            return result;
        }
        private int findPathLength(int[] path)
        {
            int sum = 0;
            for (int i = 0; i < path.Length; i++)
            {
                if (i + 1 != path.Length)
                {
                    sum += _routes[path[i], path[i + 1]];
                }
                else
                {
                    sum += _routes[path[i], path[0]];
                }
            }
            return sum;
        }
        private void AddToMatrix(int[,] matrix, int[] array, int pos)
        {
            for (int i = 0; i < _routes.GetLength(0); i++)
            {
                matrix[pos, i] = array[i];
            }
        }
        private int[] GreedyAlgo()
        {
            int tourLength = 0;
            int counter = 0;
            int i = 0, j = 0;
            int minDistance = Int32.MaxValue;
            HashSet<int> visitedCities = new HashSet<int>();

            int size = _routes.GetLength(0);

            visitedCities.Add(rand.Next(size));
            int[] path = new int[_routes.GetLength(0)];

            while (i < size && j < size)
            {
                if (counter >= size - 1)
                {
                    break;
                }

                if (j != i && !(visitedCities.Contains(j)))
                {
                    if (_routes[i, j] < minDistance)
                    {
                        minDistance = _routes[i, j];
                        path[counter] = j + 1;
                    }
                }
                j++;

                if (j == _routes.GetLength(0))
                {
                    tourLength += minDistance;
                    minDistance = Int32.MaxValue;
                    visitedCities.Add(path[counter] - 1);
                    j = 0;
                    i = path[counter] - 1;
                    counter++;
                }
            }

            i = path[counter - 1] - 1;

            for (j = 0; j < size; j++)
            {

                if ((i != j) && _routes[i, j] < minDistance)
                {
                    minDistance = _routes[i, j];
                    path[counter] = j + 1;
                }
            }
            tourLength += minDistance;
            //Console.WriteLine(tourLength);
            for (int k = 0; k < path.Length; k++)
            {
                path[k] = path[k] - 1;
            }
            var query = path.GroupBy(x => x)
              .Where(g => g.Count() > 1)
              .Select(y => y.Key)
              .ToList();
            int[] threeHundreed = new int[_routes.GetLength(0)];

            for (int k = 0; k < _routes.GetLength(0); k++)
            {
                threeHundreed[k] = k;
            }
            int missingNumber = -1;
            for (int k = 0; k < _routes.GetLength(0); k++)
            {
                if (!path.Contains(threeHundreed[k]))
                {
                    missingNumber = threeHundreed[k];
                }
            }
            for (int k = 0; k < query.Count; k++)
            {
                int count = 0;
                for (int g = 0; g < path.Length; g++)
                {
                    if (path[g] == query[k])
                    {
                        count++;
                    }
                    if (count > 1)
                    {
                        path[g] = missingNumber;
                    }
                }
            }
            return path;
        }
        public static int[,] GenerateMatrix(int size = 300)
        {
            Random rand = new Random();
            int[,] routes = new int[size, size];
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    if (i != j)
                    {
                        routes[i, j] = rand.Next(5, 151);
                    }
                    else
                    {
                        routes[i, j] = 0;
                    }
                }
            }
            return routes;
        }
        public void SaveToFile(string fileName)
        {
            StreamWriter sw = new StreamWriter(fileName);
            string a = "";
            int size = _routes.GetLength(0);
            a += $"{size} \n";
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    a += $"{_routes[i, j]} ;";
                }
                a = a.Remove(a.Length - 1);
                a += "\n";
            }
            a += $"\n Best way is: ";
            for (int i = 0; i < _path.Length; i++)
            {
                a += $"{_path[i]} ->";
            }
            a = a.Remove(a.Length - 2, a.Length - 1);
            sw.WriteLine(a);
            sw.Close();
        }
        static int[,] ReadFromFile(string fileName)
        {
            if (File.Exists(fileName))
            {
                string[] readText = File.ReadAllLines(fileName);
                int size = Convert.ToInt32(readText[0]);
                int[,] routes = new int[size, size];
                for (int i = 1; i <= size; i++)
                {
                    string[] parsed = readText[i].Split(";");
                    for (int j = 0; j < parsed.Length; j++)
                    {
                        routes[i, j] = Convert.ToInt32(parsed[j]);
                    }
                }
                return routes;
            }
            return null;
        }
    }
    public static class ArrayExt
    {
        public static T[] GetRow<T>(this T[,] array, int row)
        {
            if (!typeof(T).IsPrimitive)
                throw new InvalidOperationException("Not supported for managed types.");

            if (array == null)
                throw new ArgumentNullException("array");

            int cols = array.GetUpperBound(1) + 1;
            T[] result = new T[cols];

            int size;

            if (typeof(T) == typeof(bool))
                size = 1;
            else if (typeof(T) == typeof(char))
                size = 2;
            else
                size = Marshal.SizeOf<T>();

            Buffer.BlockCopy(array, row * cols * size, result, 0, cols * size);

            return result;
        }
    }
}
