using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Shape : MonoBehaviour
{

    private float lastShapeDown;

    private float lastKeyDown;
    private float timeKeyPressed;

    public void AlignCenter()
    {
        transform.position += transform.position - GameManager.Center(gameObject);
    }


    bool isValidGridPos()
    {
        foreach (Transform child in transform)
        {
            Vector2 v = GameManager.roundVector2(child.position);

            // Check if shape inside borders
            if (!GameManager.insideBorder(v))
            {
                return false;
            }

            // Block in grid cell
            if (GameManager.grid[(int)(v.x), (int)(v.y)] != null &&
                GameManager.grid[(int)(v.x), (int)(v.y)].parent != transform)
            {
                return false;
            }
        }

        return true;
    }

    // update the grid
    void updateGrid()
    {
        // Remove old children from grid
        for (int y = 0; y < GameManager.h; ++y)
        {
            for (int x = 0; x < GameManager.w; ++x)
            {
                if (GameManager.grid[x, y] != null &&
                    GameManager.grid[x, y].parent == transform)
                {
                    GameManager.grid[x, y] = null;
                }
            }
        }

        insertOnGrid();
    }

    void insertOnGrid()
    {
        // Add new childs to grid
        foreach (Transform child in transform)
        {
            Vector2 v = GameManager.roundVector2(child.position);
            GameManager.grid[(int)v.x, (int)v.y] = child;
        }
    }

    void gameOver()
    {
        GameManager.Instance.GameOver();

        while (!isValidGridPos())
        {
            transform.position += new Vector3(0, 1, 0);
        }
        updateGrid(); // to not overleap invalid groups
        enabled = false; // disable script when dead
    }

    void Start()
    {
        lastShapeDown = Time.time;
        lastKeyDown = Time.time;
        timeKeyPressed = Time.time;
        if (isValidGridPos())
        {
            insertOnGrid();
        }
        else
        {
            gameOver();
        }
    }

    void tryChangePos(Vector3 v)
    {
        transform.position += v;

        // See if valid
        if (isValidGridPos())
        {
            updateGrid();
        }
        else
        {
            transform.position -= v;
        }
    }

    void fallGroup()
    {
        transform.position += new Vector3(0, -1, 0);

        if (isValidGridPos())
        {
            // It's valid. Update grid.
            updateGrid();
        }
        else
        {
            // it's not valid. revert
            transform.position += new Vector3(0, 1, 0);

            // Clear filled horizontal lines
            GameManager.deleteFullRows();


            FindObjectOfType<SpawnShapes>().spawnNext();


            // Disable script
            enabled = false;
        }

        lastShapeDown = Time.time;

    }

    // getKey if is pressed now on longer pressed by 0.5 seconds | if that true apply the key each 0.05f while is pressed
    bool getKey(KeyCode key)
    {
        bool keyDown = Input.GetKeyDown(key);
        bool pressed = Input.GetKey(key) && Time.time - lastKeyDown > 0.5f && Time.time - timeKeyPressed > 0.05f;

        if (keyDown)
        {
            lastKeyDown = Time.time;
        }
        if (pressed)
        {
            timeKeyPressed = Time.time;
        }

        return keyDown || pressed;
    }


    void Update()
    {
        if (GameManager.Instance.pause)
        {
            return; // don't do nothing
        }
        if (getKey(KeyCode.LeftArrow))
        {
            tryChangePos(new Vector3(-1, 0, 0));
        }
        else if (getKey(KeyCode.RightArrow))
        {  // Move right
            tryChangePos(new Vector3(1, 0, 0));
        }
        else if (getKey(KeyCode.UpArrow) && gameObject.tag != "Cube")
        { // Rotate
            transform.Rotate(0, 0, -90);

            // see if valid
            if (isValidGridPos())
            {
                // it's valid. Update grid
                updateGrid();
            }
            else
            {
                // it's not valid. revert
                transform.Rotate(0, 0, 90);
            }
        }
        else if (getKey(KeyCode.DownArrow) || (Time.time - lastShapeDown) >= (float)1)
        {
            fallGroup();
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            while (enabled)
            { // fall until the bottom 
                fallGroup();
            }
        }

    }
}
