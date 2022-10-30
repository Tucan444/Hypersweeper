using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minefield
{
    public int dimension = 2;
    public int size = 16;
    public float density = 0.05f;
    public int[][] m2;
    public int[][][] m3;
    public int[][][][] m4;

    public float tilesAmount = 0;
    public int mines = 0;

    public Minefield(int dimension_=2, float density_=0.05f, int size_=16) {
        dimension = dimension_;
        density = density_;
        size = size_;

        tilesAmount = Mathf.Pow(size, dimension);

        switch (dimension) {
            case 2: 
                m2 = new int[size][];
                for (int i = 0; i < size; i++) {
                    int[] line = new int[size];
                    m2[i] = line;
                }
                break;
            case 3: 
                m3 = new int[size][][];
                for (int j = 0; j < size; j++) {
                    int[][] m22 = new int[size][];
                    m3[j] = m22;
                    for (int i = 0; i < size; i++) {
                        int[] line = new int[size];
                        m3[j][i] = line;
                    }
                }
                break;
            case 4: 
                m4 = new int[size][][][];
                for (int k = 0; k < size; k++) {
                    int[][][] m33 = new int[size][][];
                    m4[k] = m33;
                    for (int j = 0; j < size; j++) {
                        int[][] m22 = new int[size][];
                        m4[k][j] = m22;
                        for (int i = 0; i < size; i++) {
                            int[] line = new int[size];
                            m4[k][j][i] = line;
                        }
                    }
                }
                break;
        }

        DeployMines();
    }

    void DeployMines() {
        int mCount = (int)(tilesAmount * density);
        int[] pos;

        if (mCount >= tilesAmount) {
            Debug.Log("ERROR: density way too high");
            return;
        }

        while (mines < mCount) {
            switch (dimension) {
                case 2:
                    pos = new int[2] {Random.Range(0, size), Random.Range(0, size)};
                    if (m2[pos[0]][pos[1]] == 0) {
                        m2[pos[0]][pos[1]] = -1;
                        mines++;
                    }
                    break;
                case 3:
                    pos = new int[3] {Random.Range(0, size), Random.Range(0, size), Random.Range(0, size)};
                    if (m3[pos[0]][pos[1]][pos[2]] == 0) {
                        m3[pos[0]][pos[1]][pos[2]] = -1;
                        mines++;
                    }
                    break;
                case 4:
                    pos = new int[4] {Random.Range(0, size), Random.Range(0, size), Random.Range(0, size), Random.Range(0, size)};
                    if (m4[pos[0]][pos[1]][pos[2]][pos[3]] == 0) {
                        m4[pos[0]][pos[1]][pos[2]][pos[3]] = -1;
                        mines++;
                    }
                    break;
            }
        }
    }

    public void FreeSpace(int[] pos) {
        int[] p;
        switch (dimension) {
            case 2:
                if (m2[pos[0]][pos[1]] == -1) {
                    m2[pos[0]][pos[1]] = 0;
                    p = new int[2] {Random.Range(0, size), Random.Range(0, size)};
                    while (m2[p[0]][p[1]] != 0) {
                        p = new int[2] {Random.Range(0, size), Random.Range(0, size)};
                    }
                    m2[p[0]][p[1]] = -1;
                }
                break;
            case 3:
                if (m3[pos[0]][pos[1]][pos[2]] == -1) {
                    m3[pos[0]][pos[1]][pos[2]] = 0;
                    p = new int[3] {Random.Range(0, size), Random.Range(0, size), Random.Range(0, size)};
                    while (m3[p[0]][p[1]][p[2]] != 0) {
                        p = new int[3] {Random.Range(0, size), Random.Range(0, size), Random.Range(0, size)};
                    }
                    m3[p[0]][p[1]][p[2]] = -1;
                }
                break;
            case 4:
                if (m4[pos[0]][pos[1]][pos[2]][pos[3]] == -1) {
                    m4[pos[0]][pos[1]][pos[2]][pos[3]] = 0;
                    p = new int[4] {Random.Range(0, size), Random.Range(0, size), Random.Range(0, size), Random.Range(0, size)};
                    while (m4[p[0]][p[1]][p[2]][p[3]] != 0) {
                        p = new int[4] {Random.Range(0, size), Random.Range(0, size), Random.Range(0, size), Random.Range(0, size)};
                    }
                    m4[p[0]][p[1]][p[2]][p[3]] = -1;
                }
                break;
        }
    }

    public void CalculateTiles() {
        int bombCount;
        switch (dimension) {
            case 2:
                for (int x = 0; x < size; x++) {
                    for (int y = 0; y < size; y++) {
                        if (m2[x][y] != -1) {

                            bombCount = 0;
                            for (int i = -1; i < 2; i++) {
                                for (int j = -1; j < 2; j++) {
                                    int[] pos = new int[2] {x+i, y+j};
                                    if (isIn(pos)) {
                                        if (m2[pos[0]][pos[1]] == -1) {
                                            bombCount += 1;
                                        }
                                    }
                                }
                            }

                            m2[x][y] = bombCount;

                        }
                    }
                }
                break;
            case 3:
                for (int x = 0; x < size; x++) {
                    for (int y = 0; y < size; y++) {
                        for (int z = 0; z < size; z++) {
                            if (m3[x][y][z] != -1) {

                                bombCount = 0;
                                for (int i = -1; i < 2; i++) {
                                    for (int j = -1; j < 2; j++) {
                                        for (int k = -1; k < 2; k++) {
                                            int[] pos = new int[3] {x+i, y+j, z+k};
                                            if (isIn(pos)) {
                                                if (m3[pos[0]][pos[1]][pos[2]] == -1) {
                                                    bombCount += 1;
                                                }
                                            }
                                        }
                                    }
                                }

                                m3[x][y][z] = bombCount;

                            }
                        }
                    }
                }
                break;
            case 4:
                for (int x = 0; x < size; x++) {
                    for (int y = 0; y < size; y++) {
                        for (int z = 0; z < size; z++) {
                            for (int w = 0; w < size; w++) {
                                if (m4[x][y][z][w] != -1) {

                                    bombCount = 0;
                                    for (int i = -1; i < 2; i++) {
                                        for (int j = -1; j < 2; j++) {
                                            for (int k = -1; k < 2; k++) {
                                                for (int q = -1; q < 2; q++) {
                                                    int[] pos = new int[4] {x+i, y+j, z+k, w+q};
                                                    if (isIn(pos)) {
                                                        if (m4[pos[0]][pos[1]][pos[2]][pos[3]] == -1) {
                                                            bombCount += 1;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    m4[x][y][z][w] = bombCount;

                                }
                            }
                        }
                    }
                }
                break;
        }
    }

    public bool isIn(int[] pos) {
        for (int i = 0; i < pos.Length; i++) {
            if (pos[i] < 0 || pos[i] >= size) {
                return false;
            }
        }
        return true;
    }
}
