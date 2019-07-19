using System;
using System.Collections.Generic;

public class MaxSubRect {
    // Finds the maximum area under the  
    // histogram represented by histogram. 
    // See below article for details.  
    // https://www.geeksforgeeks.org/largest-rectangle-under-histogram/  
    public static int MaxHist(int C, int[] row) {
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

        // Run through all bars of  
        // given histogram (or row)  
        int i = 0;
        while (i < C) {
            // If this bar is higher than the  
            // bar on top stack, push it to stack  
            if (result.Count == 0 ||
                row[result.Peek()] <= row[i]) {
                result.Push(i++);
            } else {
                // If this bar is lower than top  
                // of stack, then calculate area of  
                // rectangle with stack top as  
                // the smallest (or minimum height) 
                //  bar. 'i' is 'right index' for 
                // the top and element before  
                // top in stack is 'left index'  
                top_val = row[result.Peek()];
                result.Pop();
                area = top_val * i;

                if (result.Count > 0) {
                    area = top_val * (i - result.Peek() - 1);
                }
                max_area = Math.Max(area, max_area);
            }
        }

        // Now pop the remaining bars from  
        // stack and calculate area with 
        // every popped bar as the smallest bar  
        while (result.Count > 0) {
            top_val = row[result.Peek()];
            result.Pop();
            area = top_val * i;
            if (result.Count > 0) {
                area = top_val * (i - result.Peek() - 1);
            }

            max_area = Math.Max(area, max_area);
        }
        return max_area;
    }

    // Returns area of the largest  
    // rectangle with all 1s in A[][]  
    public static int MaxRectangle(int R, int C,
                                   int[][] A) {
        // Calculate area for first row  
        // and initialize it as result  
        int result = MaxHist(C, A[0]);

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
            result = Math.Max(result, MaxHist(C, A[i]));
        }

        return result;
    }

    // Driver code  
    public static void TestCode() {
        int R = 4;
        int C = 4;

        var A = new int[][] {
            new int[] {0, 1, 1, 0},
            new int[] {1, 1, 0, 1},
            new int[] {1, 1, 1, 1},
            new int[] {1, 1, 0, 0}
        };
        Console.WriteLine("Area of maximum rectangle is " + MaxRectangle(R, C, A));
    }
}