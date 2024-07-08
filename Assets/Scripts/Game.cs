using UnityEngine;

public class Game : MonoBehaviour {
    public int width = 16;
    public int height = 16;
    public int mineCount = 32;

    private Board board;
    private Cell[,] state;
    private bool gameOver;

    private void OnValidate() {
        mineCount = Mathf.Clamp(mineCount, 0, width * height);
    }

    private void Awake() {
        board = GetComponentInChildren<Board>();
    }

    private void Start() {
        NewGame();
    }

    private void NewGame() {
        state = new Cell[width, height];
        gameOver = false;

        GenerateCells();
        GenerateMines();
        GenerateNumbers();

        Camera.main.transform.position = new Vector3(width / 2f, height / 2f, -10f);
        board.Draw(state);
    }

    private void GenerateCells() {
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                Cell cell = new Cell();
                cell.position = new Vector3Int(x, y, 0);
                cell.type = Cell.Type.Empty;
                cell.number = 0;
                state[x, y] = cell;
            }
        }
    }

    private void GenerateMines() {
        int minesPlaced = 0;

        while (minesPlaced < mineCount) {
            int x = Random.Range(0, width);
            int y = Random.Range(0, height);

            if (state[x, y].type != Cell.Type.Mine) {
                state[x, y].type = Cell.Type.Mine;
                minesPlaced++;
            }
        }
    }

    private void GenerateNumbers() {
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                if (state[x, y].type == Cell.Type.Mine) {
                    UpdateNumbers(x, y);
                }
            }
        }
    }

    private void UpdateNumbers(int x, int y) {
        for (int i = -1; i <= 1; i++) {
            int newX = x + i;
            if (newX < 0 || newX >= width) continue;
            for (int j = -1; j <= 1; j++) {
                if (i == 0 && j == 0) continue;

                int newY = y + j;
                if (newY < 0 || newY >= height) continue;

                state[newX, newY].number++;
                if (state[newX, newY].type == Cell.Type.Empty) {
                    state[newX, newY].type = Cell.Type.Number;
                }
            }
        }
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.R)) {
            NewGame();
        }

        if (!gameOver) {
            if (Input.GetMouseButtonDown(0)) {
                Reveal();
            } else if (Input.GetMouseButtonDown(1)) {
                Flag();
            }
        }
    }

    private void Reveal() {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPosition = board.tilemap.WorldToCell(worldPosition);
        Cell cell = GetCell(cellPosition.x, cellPosition.y);

        if (cell.type == Cell.Type.Invalid || cell.revealed || cell.flagged) return;

        switch (cell.type) {
            case Cell.Type.Mine:
                Explode(cell);
                break;
            case Cell.Type.Empty:
                Flood(cell);
                CheckWinCondition();
                break;
            default:
                cell.revealed = true;
                state[cellPosition.x, cellPosition.y] = cell;
                CheckWinCondition();
                break;
        }

        board.Draw(state);
    }

    private void Flag() {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPosition = board.tilemap.WorldToCell(worldPosition);
        Cell cell = GetCell(cellPosition.x, cellPosition.y);

        if (cell.type == Cell.Type.Invalid || cell.revealed) return;

        cell.flagged = !cell.flagged;
        state[cellPosition.x, cellPosition.y] = cell;
        board.Draw(state);
    }

    private void Flood(Cell cell) {
        if (cell.revealed) return;
        if (cell.type == Cell.Type.Mine || cell.type == Cell.Type.Invalid) return;

        int x = cell.position.x;
        int y = cell.position.y;
        cell.revealed = true;
        state[x, y] = cell;

        if (state[x, y].type == Cell.Type.Empty) {
            Flood(GetCell(x - 1, y));
            Flood(GetCell(x + 1, y));
            Flood(GetCell(x, y - 1));
            Flood(GetCell(x, y + 1));
        }
    }

    private void Explode(Cell cell) {
        Debug.Log("Game Over!");
        gameOver = true;

        cell.revealed = true;
        cell.exploded = true;
        state[cell.position.x, cell.position.y] = cell;

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                if (state[x, y].type == Cell.Type.Mine) {
                    state[x, y].revealed = true;
                }
            }
        }
    }

    private void CheckWinCondition() {
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                Cell cell = state[x, y];
                if (cell.type != Cell.Type.Mine && !cell.revealed) {
                    return;
                }
            }
        }

        Debug.Log("You Win!");
        gameOver = true;

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                if (state[x, y].type == Cell.Type.Mine) {
                    state[x, y].flagged = true;
                }
            }
        }
    }

    private Cell GetCell(int x, int y) {
        if (IsValid(x, y)) {
            return state[x, y];
        } else {
            return new Cell();
        }
    }

    private bool IsValid(int x, int y) {
        return x >= 0 && x < width && y >= 0 && y < height;
    }
}
