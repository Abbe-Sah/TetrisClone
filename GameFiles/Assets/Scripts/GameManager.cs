using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    // The grid itself
    public static int w = 10;
    public static int h = 20;
    // grid storing the Transform element
    public static Transform[,] grid = new Transform[w, h];


    private static GameManager _instance;
    public static GameManager Instance { get { return _instance; } }

    public int score = 0;
    public int highScore = 0;

    public TextMeshProUGUI scoreHolder;
    public TextMeshProUGUI highScoreHolder;


    public bool pause = false;
    public Image pauseWindow;  
    
    public Image gameOverWindow;

    // Start is called before the first frame update
    void Start()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(this.gameObject);

        //Get highscore saved in playerprefs.
        highScore = PlayerPrefs.GetInt("HighScore", highScore);
        highScoreHolder.text = "" + highScore;
    }

    void Update()
    {
        
    }


    // convert a real vector to discret coordinates.
    public static Vector2 roundVector2(Vector2 v)
    {
        return new Vector2(Mathf.Round(v.x), Mathf.Round(v.y));
    }

    // check if some vector is inside the limits of game (borders left, right and down)
    public static bool insideBorder(Vector2 pos)
    {
        return ((int)pos.x >= 0 &&
                (int)pos.x < w &&
                (int)pos.y >= 0);
    }

    // destroy the row at y line
    public static void deleteRow(int y)
    {
        for (int x = 0; x < w; x++)
        {
            Destroy(grid[x, y].gameObject);
            grid[x, y] = null;
        }
    }


    // Whenever a row was deleted, the above rows should fall towards the bottom by one unit. 
    public static void decreaseRow(int y)
    {
        for (int x = 0; x < w; x++)
        {
            if (grid[x, y] != null)
            {
                // move one twoards bottom
                grid[x, y - 1] = grid[x, y];
                grid[x, y] = null;

                // Update block position
                grid[x, y - 1].position += new Vector3(0, -1, 0);
            }
        }
    }

    // whenever a row is deleted, all the above rows should be descreased by 1
    public static void decreaseRowAbove(int y)
    {
        for (int i = y; i < h; i++)
        {
            decreaseRow(i);
        }
    }

    // check if a row is full and, then, can be deleted.
    public static bool isRowFull(int y)
    {
        for (int x = 0; x < w; x++)
        {
            if (grid[x, y] == null)
            {
                return false;
            }
        }
        return true;

    }

    public static void deleteFullRows()
    {
        for (int y = 0; y < h; y++)
        {
            if (isRowFull(y))
            {
                deleteRow(y);
                decreaseRowAbove(y + 1);
                // add 10 points points to score when a row is deleted
                GameManager.Instance.score += 10;
                GameManager.Instance.scoreHolder.text = "" + GameManager.Instance.score;
                --y;
                //--y decreases y by one whenever a row was deleted.
            }
        }
    }



    //Getting bounds fron the group
    public static Bounds getRenderBounds(GameObject obj)
    {
        var bounds = new Bounds(Vector3.zero, Vector3.zero);
        var render = obj.GetComponent<Renderer>();
        return render != null ? render.bounds : bounds;
    }

    // this too
    public static Bounds getBounds(GameObject obj)
    {
        Bounds bounds;
        Renderer childRender;
        bounds = getRenderBounds(obj);
        if ((int)bounds.extents.x == 0)
        {
            bounds = new Bounds(obj.transform.position, Vector3.zero);
            foreach (Transform child in obj.transform)
            {
                childRender = child.GetComponent<Renderer>();
                if (childRender)
                {
                    bounds.Encapsulate(childRender.bounds);
                }
                else
                {
                    bounds.Encapsulate(getBounds(child.gameObject));
                }
            }
        }
        return bounds;
    }

    // get the center of a gameobject
    public static Vector3 Center(GameObject obj)
    {
        return GameManager.getBounds(obj).center;
    }

    public static int Mod(int n, int m)
    {
        return ((n % m) + m) % m;
    }


    public void PauseUnpause()
    {
        pause = !pause;

        if (pause)
        {
            pauseWindow.gameObject.SetActive(true);
        }
        else
        {
            pauseWindow.gameObject.SetActive(false);
        }
        
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        Destroy(gameObject);
    }

    public void GameOver()
    {

      gameOverWindow.gameObject.SetActive(true);
        //Check if current score is higher than highscore, if so then save the new highscore in playerpref.
        if (score > highScore)
        {
            highScore = score;

            PlayerPrefs.SetInt("HighScore", highScore);
        }
        highScoreHolder.text = "" + highScore;      
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Aplication Closed");
    }
}
