﻿using Sudoku.PSO;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Sudoku.PSO
{
    public static class SudokuUtils
    {
        public static double Fitness(SudokuInstance instance, int[] path)
        {
            double cost = 0;

            for (int i = 1; i < path.Length; i++)
            {
                cost += instance.NbErrors[path[i - 1], path[i]];
            }
            cost += instance.NbErrors[path[path.Length - 1], path[0]];

            return cost;
        }

        public static int[] RandomSolution(SudokuInstance instance)
        {
            int[] solution = new int[instance.NumberSudokuGrids];
            List<int> cities = new List<int>();

            for (int city = 0; city < instance.NumberSudokuGrids; city++)
            {
                cities.Add(city);
            }
            for (int i = 0; i < instance.NumberSudokuGrids; i++)
            {
                int cityIndex = Statistics.RandomDiscreteUniform(0, cities.Count - 1);
                int city = cities[cityIndex];
                cities.RemoveAt(cityIndex);
                solution[i] = city;
            }

            return solution;
        }

        public static int[] GreedySolution(SudokuInstance instance)
        {
            int[] solution = new int[instance.NumberSudokuGrids];
            bool[] visited = new bool[instance.NumberSudokuGrids];

            for (int i = 0; i < instance.NumberSudokuGrids; i++)
            {
                if (i == 0)
                {
                    solution[i] = 0;
                }
                else
                {
                    int currentCity = solution[i - 1];
                    int nextCity;
                    double bestCost = double.MaxValue;
                    for (nextCity = 1; nextCity < instance.NumberSudokuGrids; nextCity++)
                    {
                        if (!visited[nextCity] && instance.NbErrors[currentCity, nextCity] < bestCost)
                        {
                            solution[i] = nextCity;
                            bestCost = instance.NbErrors[currentCity, nextCity];
                        }
                    }
                }
                visited[solution[i]] = true;
            }

            return solution;
        }

        public static int[] GetNeighbor(SudokuInstance instance, int[] solution)
        {
            int[] neighbor = new int[instance.NumberSudokuGrids];
            int a = Statistics.RandomDiscreteUniform(0, solution.Length - 1);
            int b = a;
            while (b == a)
            {
                b = Statistics.RandomDiscreteUniform(0, solution.Length - 1);
            }
            for (int i = 0; i < solution.Length; i++)
            {
                if (i == a)
                {
                    neighbor[i] = solution[b];
                }
                else if (i == b)
                {
                    neighbor[i] = solution[a];
                }
                else
                {
                    neighbor[i] = solution[i];
                }
            }

            return neighbor;
        }

        public static void Repair(SudokuInstance instance, int[] individual)
        {
            int visitedCitiesCount = 0;
            bool[] visitedCities = new bool[instance.NumberSudokuGrids];
            bool[] repeatedPositions = new bool[instance.NumberSudokuGrids];

            // Get information to decide if the individual is valid.
            for (int i = 0; i < instance.NumberSudokuGrids; i++)
            {
                if (!visitedCities[individual[i]])
                {
                    visitedCitiesCount += 1;
                    visitedCities[individual[i]] = true;
                }
                else
                {
                    repeatedPositions[i] = true;
                }
            }

            // If the individual is invalid, make it valid.
            if (visitedCitiesCount != instance.NumberSudokuGrids)
            {
                for (int i = 0; i < repeatedPositions.Length; i++)
                {
                    if (repeatedPositions[i])
                    {
                        int count = Statistics.RandomDiscreteUniform(1, instance.NumberSudokuGrids - visitedCitiesCount);
                        for (int c = 0; c < visitedCities.Length; c++)
                        {
                            if (!visitedCities[c])
                            {
                                count -= 1;
                                if (count == 0)
                                {
                                    individual[i] = c;
                                    repeatedPositions[i] = false;
                                    visitedCities[c] = true;
                                    visitedCitiesCount += 1;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        // Implementation of the 2-opt (first improvement) local search algorithm.
        public static void LocalSearch2OptFirst(SudokuInstance instance, int[] path)
        {
            int tmp;
            double currentFitness, bestFitness;

            bestFitness = Fitness(instance, path);
            for (int j = 1; j < path.Length; j++)
            {
                for (int i = 0; i < j; i++)
                {
                    // Swap the items.
                    tmp = path[j];
                    path[j] = path[i];
                    path[i] = tmp;

                    // Evaluate the fitness of this new solution.
                    currentFitness = Fitness(instance, path);
                    if (currentFitness < bestFitness)
                    {
                        return;
                    }

                    // Undo the swap.
                    tmp = path[j];
                    path[j] = path[i];
                    path[i] = tmp;
                }
            }
        }

        // Implementation of the 2-opt (best improvement) local search algorithm.
        public static void LocalSearch2OptBest(SudokuInstance instance, int[] path)
        {
            int tmp;
            int firstSwapItem = 0, secondSwapItem = 0;
            double currentFitness, bestFitness;

            bestFitness = Fitness(instance, path);
            for (int j = 1; j < path.Length; j++)
            {
                for (int i = 0; i < j; i++)
                {
                    // Swap the items.
                    tmp = path[j];
                    path[j] = path[i];
                    path[i] = tmp;

                    // Evaluate the fitness of this new solution.
                    currentFitness = Fitness(instance, path);
                    if (currentFitness < bestFitness)
                    {
                        firstSwapItem = j;
                        secondSwapItem = i;
                        bestFitness = currentFitness;
                    }

                    // Undo the swap.
                    tmp = path[j];
                    path[j] = path[i];
                    path[i] = tmp;
                }
            }

            // Use the best assignment.
            if (firstSwapItem != secondSwapItem)
            {
                tmp = path[firstSwapItem];
                path[firstSwapItem] = path[secondSwapItem];
                path[secondSwapItem] = tmp;
            }
        }

        // Implementation of the Tabu Movement of two movements.
        public static Tuple<int, int> GetTabu(int[] source, int[] destiny)
        {
            Tuple<int, int> tabu = new Tuple<int, int>(-1, -1);

            for (int i = 0; i < source.Length; i++)
            {
                if (source[i] != destiny[i])
                {
                    tabu.Val1 = Math.Min(source[i], destiny[i]);
                    tabu.Val2 = Math.Max(source[i], destiny[i]);
                    break;
                }
            }

            return tabu;
        }

        // Implementation of the GRC solution's construction algorithm.
        public static int[] GRCSolution(SudokuInstance instance, double rclThreshold)
        {
            int numCities = instance.NumberSudokuGrids;
            int[] path = new int[instance.NumberSudokuGrids];
            int totalCities = numCities;
            int index = 0;
            double best = 0;
            double cost = 0;
            int city = 0;
            // Restricted Candidate List.
            SortedList<double, int> rcl = new SortedList<double, int>();
            // Available cities.
            bool[] visited = new bool[numCities];

            path[0] = Statistics.RandomDiscreteUniform(0, numCities - 1);
            visited[path[0]] = true;
            numCities--;

            while (numCities > 0)
            {
                rcl = new SortedList<double, int>();
                for (int i = 0; i < totalCities; i++)
                {
                    if (!visited[i])
                    {
                        cost = instance.NbErrors[path[index], i];
                        if (rcl.Count == 0)
                        {
                            best = cost;
                            rcl.Add(cost, i);
                        }
                        else if (cost < best)
                        {
                            // The new city is the new best;
                            best = cost;
                            for (int j = rcl.Count - 1; j > 0; j--)
                            {
                                if (rcl.Keys[j] > rclThreshold * best)
                                {
                                    rcl.RemoveAt(j);
                                }
                                else
                                {
                                    break;
                                }
                            }
                            rcl.Add(cost, i);
                        }
                        else if (cost < rclThreshold * best)
                        {
                            // The new city is a mostly good candidate.
                            rcl.Add(cost, i);
                        }
                    }
                }
                city = rcl.Values[Statistics.RandomDiscreteUniform(0, rcl.Count - 1)];
                index++;
                visited[city] = true;
                path[index] = city;
                numCities--;
            }

            return path;
        }

        public static double Distance(SudokuInstance instance, int[] a, int[] b)
        {
            double distance = 0;

            for (int i = 0; i < a.Length - 1; i++)
            {
                if (a[i] != b[i] || a[i + 1] != b[i + 1])
                {
                    distance += 1;
                }
            }

            return distance;
        }

        public static void PerturbateSolution(int[] solution, int perturbations)
        {
            int point1 = 0;
            int point2 = 0;
            int tmp = 0;

            for (int i = 0; i < perturbations; i++)
            {
                point1 = Statistics.RandomDiscreteUniform(0, solution.Length - 1);
                point2 = Statistics.RandomDiscreteUniform(0, solution.Length - 1);
                tmp = solution[point1];
                solution[point1] = solution[point2];
                solution[point2] = tmp;
            }
        }
    }
}

