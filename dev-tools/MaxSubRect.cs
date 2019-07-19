using System;
using System.Collections.Generic;
using System.Diagnostics;

public class MaxSubRect {
    // Finds the maximum area under the  
    // histogram represented by histogram. 
    // See below article for details.  
    // https://www.geeksforgeeks.org/largest-rectangle-under-histogram/  
    public static int MaxHist(int[] row, out int beginIndex, out int endIndex) {
        // Create an empty stack. The stack 
        // holds indexes of hist[] array.  
        // The bars stored in stack are always  
        // in increasing order of their heights.  
        Stack<int> result = new Stack<int>();

        int top_val; // Top of stack 

        int max_area = 0; // Initialize max area in  
                          // current row (or histogram)  

        int area = 0; // Initialize area with  
                      // current top 

        beginIndex = 0;
        endIndex = 0;

        // Run through all bars of  
        // given histogram (or row)  
        int i = 0;
        while (i < row.Length) {
            // If this bar is higher than the  
            // bar on top stack, push it to stack  
            if (result.Count == 0 || row[result.Peek()] <= row[i]) {
                result.Push(i++);
            } else {
                // If this bar is lower than top of stack,
                // then calculate area of rectangle with stack top as  the smallest (or minimum height) bar.
                // 'i' is 'right index' for the top and
                // element before top in stack is 'left index' 
                top_val = row[result.Peek()];
                result.Pop();
                area = top_val * i;
                var newBeginIndex = 0;

                if (result.Count > 0) {
                    area = top_val * (i - result.Peek() - 1);
                    newBeginIndex = result.Peek() + 1;
                }

                if (max_area < area) {
                    max_area = area;
                    beginIndex = newBeginIndex;
                    endIndex = i;
                }

            }
        }

        // Now pop the remaining bars from  
        // stack and calculate area with 
        // every popped bar as the smallest bar  
        while (result.Count > 0) {
            top_val = row[result.Peek()];
            result.Pop();
            area = top_val * i;
            var newBeginIndex = 0;

            if (result.Count > 0) {
                area = top_val * (i - result.Peek() - 1);
                newBeginIndex = result.Peek() + 1;
            }

            if (max_area < area) {
                max_area = area;
                beginIndex = newBeginIndex;
                endIndex = i;
            }
        }

        return max_area;
    }

    // Returns area of the largest  
    // rectangle with all 1s in A[][]  
    public static int MaxRectangle(int R, int C, int[][] A, out int beginIndexR, out int endIndexR, out int beginIndexC, out int endIndexC) {
        // Calculate area for first row  
        // and initialize it as result  
        int result = MaxHist(A[0], out var beginIndex, out var endIndex);
        if (result == 0) {
            beginIndexR = 0;
            endIndexR = 0;
            beginIndexC = 0;
            endIndexC = 0;
        } else {
            beginIndexR = 0;
            endIndexR = 1;
            beginIndexC = beginIndex;
            endIndexC = endIndex;
        }

        // iterate over row to find 
        // maximum rectangular area  
        // considering each row as histogram  
        for (int i = 1; i < R; i++) {
            for (int j = 0; j < C; j++) {

                // if A[i][j] is 1 then 
                // add A[i -1][j]  
                if (A[i][j] == 1) {
                    A[i][j] += A[i - 1][j];
                }
            }

            // Update result if area with current  
            // row (as last row of rectangle) is more  
            var newResult = MaxHist(A[i], out beginIndex, out endIndex);
            //Console.WriteLine($"result: {newResult} / {beginIndex} - {endIndex}");
            if (result < newResult) {
                result = newResult;

                var height = newResult / (endIndex - beginIndex);

                endIndexR = i + 1;
                beginIndexR = endIndexR - height;
                beginIndexC = beginIndex;
                endIndexC = endIndex;
            }
        }

        return result;
    }

    // Driver code  
    public static void TestMaxRectangle() {
        int R = 6;
        int C = 4;

        var A = new int[][] {
            new int[] {0, 1, 1, 1},
            new int[] {0, 1, 1, 0},
            new int[] {1, 1, 1, 1},
            new int[] {1, 1, 1, 0},
            new int[] {1, 1, 1, 0},
            new int[] {1, 1, 1, 0},
        };
        var area = MaxRectangle(R, C, A, out var beginIndexR, out var endIndexR, out var beginIndexC, out var endIndexC);
        Console.WriteLine($"Area of maximum rectangle is {area}");
        Console.WriteLine($"Coordinates range is ({beginIndexR},{beginIndexC})-({endIndexR},{endIndexC})");
    }

    static void AreEqual(int actual, int expected) {
        Debug.Assert(actual == expected, $"Expected: {expected}, Actual: {actual}");
    }

    public static void TestMaxHist() {
        var beginIndex = 0;
        var endIndex = 0;
        var area = 0;
        area = MaxSubRect.MaxHist(new int[] { 100, 0, 6, 2, 5, 4, 5, 1, 6 }, out beginIndex, out endIndex);
        AreEqual(area, 100);
        AreEqual(beginIndex, 0);
        AreEqual(endIndex, 1);
        area = MaxSubRect.MaxHist(new int[] { 6, 2, 5, 4, 5, 1, 6 }, out beginIndex, out endIndex);
        AreEqual(area, 12);
        AreEqual(beginIndex, 2);
        AreEqual(endIndex, 5);
        area = MaxSubRect.MaxHist(new int[] { 6, 2, 5, 4, 5, 1, 6, 100 }, out beginIndex, out endIndex);
        AreEqual(area, 100);
        AreEqual(beginIndex, 7);
        AreEqual(endIndex, 8);
        area = MaxSubRect.MaxHist(new int[] { 100, 100, 6, 2, 5, 4, 5, 1, 6 }, out beginIndex, out endIndex);
        AreEqual(area, 200);
        AreEqual(beginIndex, 0);
        AreEqual(endIndex, 2);
        area = MaxSubRect.MaxHist(new int[] { 100, 100, 100, 100, 200, 200, 5, 1, 6 }, out beginIndex, out endIndex);
        AreEqual(area, 600);
        AreEqual(beginIndex, 0);
        AreEqual(endIndex, 6);
        area = MaxSubRect.MaxHist(new int[] { 1 }, out beginIndex, out endIndex);
        AreEqual(area, 1);
        AreEqual(beginIndex, 0);
        AreEqual(endIndex, 1);
        area = MaxSubRect.MaxHist(new int[] { 5, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, out beginIndex, out endIndex);
        AreEqual(area, 10);
        AreEqual(beginIndex, 0);
        AreEqual(endIndex, 10);
        area = MaxSubRect.MaxHist(new int[] { 1, 1, 1, 1, 1, 5, 1, 1, 1, 1 }, out beginIndex, out endIndex);
        AreEqual(area, 10);
        AreEqual(beginIndex, 0);
        AreEqual(endIndex, 10);
        area = MaxSubRect.MaxHist(new int[] { 1, 1, 1, 1, 1, 1, 1, 1, 5, 1 }, out beginIndex, out endIndex);
        AreEqual(area, 10);
        AreEqual(beginIndex, 0);
        AreEqual(endIndex, 10);
        area = MaxSubRect.MaxHist(new int[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 5 }, out beginIndex, out endIndex);
        AreEqual(area, 10);
        AreEqual(beginIndex, 0);
        AreEqual(endIndex, 10);
        area = MaxSubRect.MaxHist(new int[] { 1, 1, 5 }, out beginIndex, out endIndex);
        AreEqual(area, 5);
        AreEqual(beginIndex, 2);
        AreEqual(endIndex, 3);
        area = MaxSubRect.MaxHist(new int[] { 1, 7, 2 }, out beginIndex, out endIndex);
        AreEqual(area, 7);
        AreEqual(beginIndex, 1);
        AreEqual(endIndex, 2);
        area = MaxSubRect.MaxHist(new int[] { 1, 7, 2, 8, 3 }, out beginIndex, out endIndex);
        AreEqual(area, 8);
        AreEqual(beginIndex, 3);
        AreEqual(endIndex, 4);
        area = MaxSubRect.MaxHist(new int[] { 1, 7, 2, 8, 3, 1, 1, 1, 1, 1, 1, 1, 1 }, out beginIndex, out endIndex);
        AreEqual(area, 13);
        AreEqual(beginIndex, 0);
        AreEqual(endIndex, 13);
        area = MaxSubRect.MaxHist(new int[] { }, out beginIndex, out endIndex);
        AreEqual(area, 0);
        AreEqual(beginIndex, 0);
        AreEqual(endIndex, 0);
        area = MaxSubRect.MaxHist(new int[] { 1, 0, 1 }, out beginIndex, out endIndex);
        AreEqual(area, 1);
        AreEqual(beginIndex, 0);
        AreEqual(endIndex, 1);
    }
}