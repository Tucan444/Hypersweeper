using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minefeels {
    // classes to be used
    SquareMaker sm;
    TextMaker tm;

    // basic properties
    public Vector2 origin = new Vector2();

    public int dimension = 2;
    public int size = 16;
    public float density = 0.05f;

    public Minefield minefield;

    // hidden
    public int[][] m2; // 0-hidden, 1-revealed, 2-marked
    public int[][][] m3;
    public int[][][][] m4;

    // objects
    public GameObject[] squares;
    public GameObject[] texts;

    // padding
    public float padding = 0.1f;
    public float squarding = 0.5f;
    public float tileSize = 1;
    public float trueTileSize;
    public float chunkSize;

    public float length = 0;
    public float width = 0;
    public float height = 0;
    public Vector2 toffset = new Vector2();

    // coloring
    Color red = new Color32(255, 95, 106, 255);
    Color yellow = new Color32(255, 249, 95, 255);
    Color orange = new Color32(255, 164, 93, 255);

    Color blue = new Color32(86, 101, 218, 255);
    Color bluec = new Color32(94, 155, 222, 255);
    Color cyan = new Color32(94, 222, 206, 255);

    Color gray = new Color32(60, 60, 60, 255);
    Color lgray = new Color32(90, 90, 90, 228);
    Color llgray = new Color32(60, 60, 60, 255);//new Color32(100, 100, 100, 255);

    Color black = new Color(0, 0, 0, 1);

    // distance showing
    int[] anchor;

    // tapping
    public bool inability = false;
    public int deaths = 0;

    // propagation
    Queue<int> reveal = new Queue<int>();
    bool[] visited;
    public int marked = 0;
    public int totalVisited = 0;

    public Minefeels(SquareMaker sm_, TextMaker tm_, int dimension_=2, int size_=16, float density_=0.05f) {
        sm = sm_;
        tm = tm_;
        dimension = dimension_;
        density = density_;
        size = size_;

        minefield = new Minefield(dimension, density, size);

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

        // padding
        trueTileSize = tileSize + (2*padding);
        chunkSize = trueTileSize * size + squarding;

        length = chunkSize * size - squarding;

        width = dimension < 4 ? chunkSize - squarding : length;
        height = dimension < 3 ? chunkSize - squarding : length;

        origin = new Vector2(-width * 0.5f, -height * 0.5f);
        toffset = new Vector2(tileSize * 0.5f, tileSize * 0.5f);

        // creating tiles && text
        squares = new GameObject[(int)minefield.tilesAmount];
        texts = new GameObject[(int)minefield.tilesAmount];
        for (int i = 0; i < squares.Length; i++)
        {
            Vector2 tpos = FieldToWorld(IndexToPos(i));
            squares[i] = sm.GetSquare(tpos, tileSize);
            texts[i] = tm.GetText(tpos + toffset);
        }
        MarkTexts();

        // creating anchor
        anchor = new int[dimension];
        for (int i = 0; i < anchor.Length; i++)
        {
            anchor[i] = -5;
        }

        // propagation
        visited = new bool[(int)minefield.tilesAmount];

        // bubling
        Bubble(minefield.mid);
    }
    
    // position transformations and boundaries
    public Vector2 FieldToWorld(int[] pos) {
        Vector2 worldPos = new Vector2(padding, padding);

        switch (pos.Length) {
            case 2:
                worldPos += new Vector2(pos[0] * trueTileSize, pos[1] * trueTileSize);
                break;
            case 3:
                worldPos += new Vector2(pos[0] * trueTileSize, pos[1] * trueTileSize + pos[2] * chunkSize);
                break;
            case 4:
                worldPos += new Vector2(pos[0] * trueTileSize + pos[3] * chunkSize, pos[1] * trueTileSize + pos[2] * chunkSize);
                break;
        }

        return worldPos + origin;
    }

    public int[] WorldToField(Vector2 worldPos) {
        Vector2 pos = worldPos - origin;
        int[] p = new int[2];

        switch (dimension) {
            case 2:
                p = new int[2]{
                    (int)Mathf.Floor(pos.x / trueTileSize),
                    (int)Mathf.Floor(pos.y / trueTileSize)
                };
                break;
            case 3:
                p = new int[3]{
                    (int)Mathf.Floor(pos.x / trueTileSize),
                    (int)Mathf.Floor((pos.y % chunkSize) / trueTileSize),
                    (int)Mathf.Floor(pos.y / chunkSize)
                };
                break;
            case 4:
                p = new int[4]{
                    (int)Mathf.Floor((pos.x % chunkSize) / trueTileSize),
                    (int)Mathf.Floor((pos.y % chunkSize) / trueTileSize),
                    (int)Mathf.Floor(pos.y / chunkSize),
                    (int)Mathf.Floor(pos.x / chunkSize)
                };
                break;
        }

        return p;
    }

    public bool isIn(Vector2 pos) {
        Vector2 p = pos - origin;

        if (p.x < 0 || p.x > length || p.y < 0 || p.y > length) { return false; }

        return true;
    }

    public int PosToIndex(int[] pos) {
        int index = 0;
        for (int i = 0; i < pos.Length; i++) {
            index += pos[i] * (int)Mathf.Pow(size, i);
        }
        return index;
    }

    public int[] IndexToPos(int index) {
        int[] pos = new int[dimension];
        for (int i = 0; i < dimension; i++)
        {
            pos[i] = (int)Mathf.Floor((index % (int)Mathf.Pow(size, i+1)) / Mathf.Pow(size, i));
        }
        return pos;
    }

    public void ChangeAnchor(int[] na) {
        // clearing already done stuff

        int[] pos;

        switch (dimension) {
            case 2:
                for (int i = -2; i < 3; i++) {
                    for (int j = -2; j < 3; j++) {
                        pos = new int[2] {anchor[0]+i, anchor[1]+j};
                        if (minefield.isIn(pos)) {
                            
                            int ind = PosToIndex(pos);
                            if (m2[pos[0]][pos[1]] == 0) {
                                sm.ChangeColor(squares[ind], red);
                            } else if (m2[pos[0]][pos[1]] == 1) {
                                sm.ChangeColor(squares[ind], gray);
                            } else if (m2[pos[0]][pos[1]] == 2) {
                                sm.ChangeColor(squares[ind], blue);
                            }

                        }
                    }
                }
                break;
            case 3:
                for (int i = -2; i < 3; i++) {
                    for (int j = -2; j < 3; j++) {
                        for (int k = -2; k < 3; k++) {
                            pos = new int[3] {anchor[0]+i, anchor[1]+j, anchor[2]+k};
                            if (minefield.isIn(pos)) {

                                int ind = PosToIndex(pos);
                                if (m3[pos[0]][pos[1]][pos[2]] == 0) {
                                    sm.ChangeColor(squares[ind], red);
                                } else if (m3[pos[0]][pos[1]][pos[2]] == 1) {
                                    sm.ChangeColor(squares[ind], gray);
                                } else if (m3[pos[0]][pos[1]][pos[2]] == 2) {
                                    sm.ChangeColor(squares[ind], blue);
                                }

                            }
                        }
                    }
                }
                break;
            case 4:
                for (int i = -2; i < 3; i++) {
                    for (int j = -2; j < 3; j++) {
                        for (int k = -2; k < 3; k++) {
                            for (int q = -2; q < 3; q++) {
                                pos = new int[4] {anchor[0]+i, anchor[1]+j, anchor[2]+k, anchor[3]+q};
                                if (minefield.isIn(pos)) {

                                    int ind = PosToIndex(pos);
                                    if (m4[pos[0]][pos[1]][pos[2]][pos[3]] == 0) {
                                        sm.ChangeColor(squares[ind], red);
                                    } else if (m4[pos[0]][pos[1]][pos[2]][pos[3]] == 1) {
                                        sm.ChangeColor(squares[ind], gray);
                                    } else if (m4[pos[0]][pos[1]][pos[2]][pos[3]] == 2) {
                                        sm.ChangeColor(squares[ind], blue);
                                    }

                                }
                            }
                        }
                    }
                }
                break;
        }

        switch (dimension) {
            case 2:
                for (int i = -2; i < 3; i++) {
                    for (int j = -2; j < 3; j++) {
                        pos = new int[2] {na[0]+i, na[1]+j};
                        if (minefield.isIn(pos)) {
                            
                            int ind = PosToIndex(pos);
                            int distance = Mathf.Max(Mathf.Abs(i), Mathf.Abs(j));

                            if (distance > 0) {
                                if (m2[pos[0]][pos[1]] == 0) {
                                    sm.ChangeColor(squares[ind], distance == 1 ? yellow : orange);
                                } else if (m2[pos[0]][pos[1]] == 1) {
                                    sm.ChangeColor(squares[ind], distance == 1 ? lgray : llgray);
                                } else if (m2[pos[0]][pos[1]] == 2) {
                                    sm.ChangeColor(squares[ind], distance == 1 ? bluec : cyan);
                                } 
                            }

                        }
                    }
                }
                break;
            case 3:
                for (int i = -2; i < 3; i++) {
                    for (int j = -2; j < 3; j++) {
                        for (int k = -2; k < 3; k++) {
                            pos = new int[3] {na[0]+i, na[1]+j, na[2]+k};
                            if (minefield.isIn(pos)) {

                                int ind = PosToIndex(pos);
                                int distance = Mathf.Max(Mathf.Abs(k), Mathf.Max(Mathf.Abs(i), Mathf.Abs(j)));

                                if (distance > 0) {
                                    if (m3[pos[0]][pos[1]][pos[2]] == 0) {
                                        sm.ChangeColor(squares[ind], distance == 1 ? yellow : orange);
                                    } else if (m3[pos[0]][pos[1]][pos[2]] == 1) {
                                        sm.ChangeColor(squares[ind], distance == 1 ? lgray : llgray);
                                    } else if (m3[pos[0]][pos[1]][pos[2]] == 2) {
                                        sm.ChangeColor(squares[ind], distance == 1 ? bluec : cyan);
                                    }
                                }
                                
                            }
                        }
                    }
                }
                break;
            case 4:
                for (int i = -2; i < 3; i++) {
                    for (int j = -2; j < 3; j++) {
                        for (int k = -2; k < 3; k++) {
                            for (int q = -2; q < 3; q++) {
                                pos = new int[4] {na[0]+i, na[1]+j, na[2]+k, na[3]+q};
                                if (minefield.isIn(pos)) {

                                    int ind = PosToIndex(pos);
                                    int distance = Mathf.Max(Mathf.Abs(q), Mathf.Max(Mathf.Abs(k), Mathf.Max(Mathf.Abs(i), Mathf.Abs(j))));

                                    if (distance > 0) {
                                        if (m4[pos[0]][pos[1]][pos[2]][pos[3]] == 0) {
                                            sm.ChangeColor(squares[ind], distance == 1 ? yellow : orange);
                                        } else if (m4[pos[0]][pos[1]][pos[2]][pos[3]] == 1) {
                                            sm.ChangeColor(squares[ind], distance == 1 ? lgray : llgray);
                                        } else if (m4[pos[0]][pos[1]][pos[2]][pos[3]] == 2) {
                                            sm.ChangeColor(squares[ind], distance == 1 ? bluec : cyan);
                                        }
                                    }

                                }
                            }
                        }
                    }
                }
                break;
        }

        anchor = na;
    }

    public void MarkTexts() {
        for (int i = 0; i < texts.Length; i++) {
            int n = minefield.GetAt(IndexToPos(i));
            tm.ChangeText(texts[i], n);
        }
    }

    public void CheckAround(int[] p) {
        int[] pos;
        int mCount = minefield.GetAt(p);
        int marked = 0;
        bool wrongFound = false;

        Queue<int> toEnqueue = new Queue<int>();

        switch (dimension) {
            case 2:
                for (int i = -1; i < 2; i++) {
                    for (int j = -1; j < 2; j++) {
                        pos = new int[2] {p[0]+i, p[1]+j};
                        if (minefield.isIn(pos)) {
                            
                            int ind = PosToIndex(pos);
                            int n = minefield.GetAt(pos);
                            switch (n) {
                                case -1:
                                    if (m2[pos[0]][pos[1]] == 0) {
                                        wrongFound = true;
                                    }
                                    break;
                                default:
                                    if (m2[pos[0]][pos[1]] == 0 && !visited[ind]) {
                                        toEnqueue.Enqueue(ind);
                                    }
                                    break;
                            }

                            if (m2[pos[0]][pos[1]] == 2) {
                                marked++;
                            }

                        }
                    }
                }
                break;
            case 3:
                for (int i = -1; i < 2; i++) {
                    for (int j = -1; j < 2; j++) {
                        for (int k = -1; k < 2; k++) {
                            pos = new int[3] {p[0]+i, p[1]+j, p[2]+k};
                            if (minefield.isIn(pos)) {

                                int ind = PosToIndex(pos);
                                int n = minefield.GetAt(pos);
                                switch (n) {
                                    case -1:
                                        if (m3[pos[0]][pos[1]][pos[2]] == 0) {
                                            wrongFound = true;
                                        }
                                        break;
                                    default:
                                        if (m3[pos[0]][pos[1]][pos[2]] == 0 && !visited[ind]) {
                                            toEnqueue.Enqueue(ind);
                                        }
                                        break;
                                }

                                if (m3[pos[0]][pos[1]][pos[2]] == 2) {
                                    marked++;
                                }

                            }
                        }
                    }
                }
                break;
            case 4:
                for (int i = -1; i < 2; i++) {
                    for (int j = -1; j < 2; j++) {
                        for (int k = -1; k < 2; k++) {
                            for (int q = -1; q < 2; q++) {
                                pos = new int[4] {p[0]+i, p[1]+j, p[2]+k, p[3]+q};
                                if (minefield.isIn(pos)) {

                                    int ind = PosToIndex(pos);
                                    int n = minefield.GetAt(pos);
                                    switch (n) {
                                        case -1:
                                            if (m4[pos[0]][pos[1]][pos[2]][pos[3]] == 0) {
                                                wrongFound = true;
                                            }
                                            break;
                                        default:
                                            if (m4[pos[0]][pos[1]][pos[2]][pos[3]] == 0 && !visited[ind]) {
                                                toEnqueue.Enqueue(ind);
                                            }
                                            break;
                                    }

                                    if (m4[pos[0]][pos[1]][pos[2]][pos[3]] == 2) {
                                        marked++;
                                    }

                                }
                            }
                        }
                    }
                }
                break;
        }

        if (marked == mCount && wrongFound) {
            deaths++;
        } else if  (marked == mCount) {
            while (toEnqueue.Count != 0) {
                int ind = toEnqueue.Dequeue();
                reveal.Enqueue(ind);
                visited[ind] = true;
                totalVisited++;
            }
        }
    }

    public void RevealOne(int[] pos, int index) {
        visited[index] = true;
        switch (dimension) {
            case 2:
                m2[pos[0]][pos[1]] = 1;
                sm.ChangeColor(squares[index], gray);
                ChangeAnchor(anchor);
                tm.ToggleText(texts[index]);
                break;
            case 3:
                m3[pos[0]][pos[1]][pos[2]] = 1;
                sm.ChangeColor(squares[index], gray);
                ChangeAnchor(anchor);
                tm.ToggleText(texts[index]);
                break;
            case 4:
                m4[pos[0]][pos[1]][pos[2]][pos[3]] = 1;
                sm.ChangeColor(squares[index], gray);
                ChangeAnchor(anchor);
                tm.ToggleText(texts[index]);
                break;
        }
    }

    public void RevealNeighbours(int[] p, int index) {
        int[] pos;

        switch (dimension) {
            case 2:
                for (int i = -1; i < 2; i++) {
                    for (int j = -1; j < 2; j++) {
                        pos = new int[2] {p[0]+i, p[1]+j};
                        if (minefield.isIn(pos) && !(j==i && i == 0)) {
                            
                            int ind = PosToIndex(pos);
                            if (m2[pos[0]][pos[1]] == 0 && !visited[ind]) {
                                reveal.Enqueue(ind);
                                visited[ind] = true;
                                totalVisited++;
                            }

                        }
                    }
                }
                break;
            case 3:
                for (int i = -1; i < 2; i++) {
                    for (int j = -1; j < 2; j++) {
                        for (int k = -1; k < 2; k++) {
                            pos = new int[3] {p[0]+i, p[1]+j, p[2]+k};
                            if (minefield.isIn(pos) && !(k==i && j==i && i == 0)) {

                                int ind = PosToIndex(pos);
                                if (m3[pos[0]][pos[1]][pos[2]] == 0 && !visited[ind]) {
                                    reveal.Enqueue(ind);
                                    visited[ind] = true;
                                    totalVisited++;
                                }

                            }
                        }
                    }
                }
                break;
            case 4:
                for (int i = -1; i < 2; i++) {
                    for (int j = -1; j < 2; j++) {
                        for (int k = -1; k < 2; k++) {
                            for (int q = -1; q < 2; q++) {
                                pos = new int[4] {p[0]+i, p[1]+j, p[2]+k, p[3]+q};
                                if (minefield.isIn(pos) && !(q==i && k==i && j==i && i == 0)) {

                                    int ind = PosToIndex(pos);
                                    if (m4[pos[0]][pos[1]][pos[2]][pos[3]] == 0 && !visited[ind]) {
                                        reveal.Enqueue(ind);
                                        visited[ind] = true;
                                        totalVisited++;
                                    }

                                }
                            }
                        }
                    }
                }
                break;
        }
    }

    public void Bubble(int[] p) {
        bool endcube = false;
        int[] pos;

        for (int f = 1; f < (int)(size*0.5f) + 1; f++) {
            switch (dimension) {
                case 2:
                    for (int i = -f; i < f+1; i+=f) {
                        for (int j = -f; j < f+1; j+=f) {
                            pos = new int[2] {p[0]+i, p[1]+j};
                            if (minefield.isIn(pos)) {
                                
                                int index = PosToIndex(pos);
                                if (minefield.GetAt(pos) == 0) {
                                    reveal.Enqueue(index);
                                    visited[index] = true;
                                    totalVisited++;
                                    endcube = true;
                                }

                            }

                            if (endcube) {
                                break;
                            }
                        }
                        if (endcube) {
                            break;
                        }
                    }
                    break;
                case 3:
                    for (int i = -f; i < f+1; i+=f) {
                        for (int j = -f; j < f+1; j+=f) {
                            for (int k = -f; k < f+1; k+=f) {
                                pos = new int[3] {p[0]+i, p[1]+j, p[2]+k};
                                if (minefield.isIn(pos)) {

                                    int index = PosToIndex(pos);
                                    if (minefield.GetAt(pos) == 0) {
                                        reveal.Enqueue(index);
                                        visited[index] = true;
                                        totalVisited++;
                                        endcube = true;
                                    }

                                }
                                if (endcube) {
                                    break;
                                }
                            }
                            if (endcube) {
                                break;
                            }
                        }
                        if (endcube) {
                            break;
                        }
                    }
                    break;
                case 4:
                    for (int i = -f; i < f+1; i+=f) {
                        for (int j = -f; j < f+1; j+=f) {
                            for (int k = -f; k < f+1; k+=f) {
                                for (int q = -f; q < f+1; q+=f) {
                                    pos = new int[4] {p[0]+i, p[1]+j, p[2]+k, p[3]+q};
                                    if (minefield.isIn(pos)) {

                                        int index = PosToIndex(pos);
                                        if (minefield.GetAt(pos) == 0) {
                                            reveal.Enqueue(index);
                                            visited[index] = true;
                                            totalVisited++;
                                            endcube = true;
                                        }

                                    }
                                    if (endcube) {
                                        break;
                                    }
                                }
                                if (endcube) {
                                    break;
                                }
                            }
                            if (endcube) {
                                break;
                            }
                        }
                        if (endcube) {
                            break;
                        }
                    }
                    break;
            }

            if (endcube) {
                break;
            }
        }
    }

    public void RevealMines() {
        for (int i = 0; i < (int)minefield.tilesAmount; i++) {
            int[] pos = IndexToPos(i);
            if (GetAt(pos) == 0 && minefield.GetAt(pos) == -1) {
                sm.ChangeColor(squares[i], black);
            }
        }
    }

    public void Tap(Vector2 pos_) {
        if (!inability) {
            int[] pos = WorldToField(pos_);
            if (minefield.isIn(pos)) {

                // processing tap
                int index = PosToIndex(pos);
                switch (dimension) {
                    case 2:
                        if (m2[pos[0]][pos[1]] == 0) {
                            if (minefield.m2[pos[0]][pos[1]] != -1) {
                                reveal.Enqueue(index);
                                visited[index] = true;
                                totalVisited++;
                            } else {
                                sm.ChangeColor(squares[index], black);
                                deaths++;
                            }
                        } else if (m2[pos[0]][pos[1]] == 1) {
                            CheckAround(pos);
                        } else if (m2[pos[0]][pos[1]] == 2) {
                            ChangeAnchor(pos);
                        }
                        break;
                    case 3:
                        if (m3[pos[0]][pos[1]][pos[2]] == 0) {
                            if (minefield.m3[pos[0]][pos[1]][pos[2]] != -1) {
                                reveal.Enqueue(index);
                                visited[index] = true;
                                totalVisited++;
                            } else {
                                sm.ChangeColor(squares[index], black);
                                deaths++;
                            }
                        } else if (m3[pos[0]][pos[1]][pos[2]] == 1) {
                            CheckAround(pos);
                        } else if (m3[pos[0]][pos[1]][pos[2]] == 2) {
                            ChangeAnchor(pos);
                        }
                        break;
                    case 4:
                        if (m4[pos[0]][pos[1]][pos[2]][pos[3]] == 0) {
                            if (minefield.m4[pos[0]][pos[1]][pos[2]][pos[3]] != -1) {
                                reveal.Enqueue(index);
                                visited[index] = true;
                                totalVisited++;
                            } else {
                                sm.ChangeColor(squares[index], black);
                                deaths++;
                            }
                        } else if (m4[pos[0]][pos[1]][pos[2]][pos[3]] == 1) {
                            CheckAround(pos);
                        } else if (m4[pos[0]][pos[1]][pos[2]][pos[3]] == 2) {
                            ChangeAnchor(pos);
                        }
                        break;
                }
            }
        }
    }

    public void Hold(Vector2 pos_) {
        if (!inability) {
            int[] pos = WorldToField(pos_);
            if (minefield.isIn(pos)) {
                int index = PosToIndex(pos);
                switch (dimension) {
                    case 2:
                        if (m2[pos[0]][pos[1]] == 0) {
                            m2[pos[0]][pos[1]] = 2;
                            sm.ChangeColor(squares[index], blue);
                            ChangeAnchor(anchor);
                            marked++;
                        } else if (m2[pos[0]][pos[1]] == 1) {
                            ChangeAnchor(pos);
                        } else if (m2[pos[0]][pos[1]] == 2) {
                            m2[pos[0]][pos[1]] = 0;
                            sm.ChangeColor(squares[index], red);
                            ChangeAnchor(anchor);
                            marked--;
                        }
                        break;
                    case 3:
                        if (m3[pos[0]][pos[1]][pos[2]] == 0) {
                            m3[pos[0]][pos[1]][pos[2]] = 2;
                            sm.ChangeColor(squares[index], blue);
                            ChangeAnchor(anchor);
                            marked++;
                        } else if (m3[pos[0]][pos[1]][pos[2]] == 1) {
                            ChangeAnchor(pos);
                        } else if (m3[pos[0]][pos[1]][pos[2]] == 2) {
                            m3[pos[0]][pos[1]][pos[2]] = 0;
                            sm.ChangeColor(squares[index], red);
                            ChangeAnchor(anchor);
                            marked--;
                        }
                        break;
                    case 4:
                        if (m4[pos[0]][pos[1]][pos[2]][pos[3]] == 0) {
                            m4[pos[0]][pos[1]][pos[2]][pos[3]] = 2;
                            sm.ChangeColor(squares[index], blue);
                            ChangeAnchor(anchor);
                            marked++;
                        } else if (m4[pos[0]][pos[1]][pos[2]][pos[3]] == 1) {
                            ChangeAnchor(pos);
                        } else if (m4[pos[0]][pos[1]][pos[2]][pos[3]] == 2) {
                            m4[pos[0]][pos[1]][pos[2]][pos[3]] = 0;
                            sm.ChangeColor(squares[index], red);
                            ChangeAnchor(anchor);
                            marked--;
                        }
                        break;
                }
            }
        }
    } 

    public void Update() {
        int rc = reveal.Count;
        int index = 0;
        int[] pos;
        for (int i = 0; i < rc; i++)
        {
            index = reveal.Dequeue();
            pos = IndexToPos(index);

            int n = minefield.GetAt(pos);
            if (n > 0) {
                RevealOne(pos, index);
            } else {
                RevealOne(pos, index);
                RevealNeighbours(pos, index);
            }
        }
    }

    public int GetAt(int[] pos) {
        int result = -1;
        switch(dimension) {
            case 2:
                result = m2[pos[0]][pos[1]];
                break;
            case 3:
                result = m3[pos[0]][pos[1]][pos[2]];
                break;
            case 4:
                result = m4[pos[0]][pos[1]][pos[2]][pos[3]];
                break;
        }

        return result;
    }
}
