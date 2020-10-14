using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneManager : MonoBehaviour {
    private Actor tester1, tester2;
    private Movement move1, move2;
    private int PathCompleteCount = 2;
    private Coord[,] tests;
    private int testI = 1;
    private int testFail = 0;

    // Use this for initialization
    void Start () {
        tester1 = GameManager.instance.ActorDictionary["d33fe0e9-2b08-4876-b2c4-a82b8b05af43"];
        tester2 = GameManager.instance.ActorDictionary["a0b2566f-47ae-4dbf-b4a8-7a52a772260b"];
        move1 = tester1.myMovement;
        move2 = tester2.myMovement;
        GameManager.instance.pathManager.OnPathComplete += RunTest;

        tests = new Coord[3, 6];
        tests[0, 0] = new Coord(7, 10);
        tests[0, 1] = new Coord(13, 10);
        tests[0, 2] = new Coord(10, 10);
        tests[0, 3] = new Coord(10, 10);
        tests[0, 4] = new Coord(14, 10);
        tests[0, 5] = new Coord(6, 10);

        tests[1, 0] = new Coord(7, 7);
        tests[1, 1] = new Coord(13, 13);
        tests[1, 2] = new Coord(10, 10);
        tests[1, 3] = new Coord(10, 10);
        tests[1, 4] = new Coord(14, 14);
        tests[1, 5] = new Coord(6, 6);

        tests[2, 0] = new Coord(7, 13);
        tests[2, 1] = new Coord(13, 7);
        tests[2, 2] = new Coord(10, 10);
        tests[2, 3] = new Coord(10, 10);
        tests[2, 4] = new Coord(14, 6);
        tests[2, 5] = new Coord(6, 14);

        StartCoroutine(WaitForReady());
    }


    private IEnumerator WaitForReady()
    {
        while (!GameManager.instance.ready || !tester1.ready || !tester2.ready)
            yield return null;
        RunTest(null);
    }

    private void RunTest( Actor actor )
    {
        PathCompleteCount++;
        if (PathCompleteCount < 2)
            return;
        PathCompleteCount = 0;
        testFail++;

        if (testFail > 3)
            return;

        move1.PlaceActor(tests[testI,0]);
        move2.PlaceActor(tests[testI, 1]);

        GameManager.instance.pathManager.MoveActorTo(tester1, tests[testI, 2]);
        GameManager.instance.pathManager.MoveActorTo(tester2, tests[testI, 3]);

        GameManager.instance.pathManager.MoveActorTo(tester1, tests[testI, 4]);
        GameManager.instance.pathManager.MoveActorTo(tester2, tests[testI, 5]);

        UDF.Swap<Actor>(ref tester1, ref tester2);
        UDF.Swap<Movement>(ref move1, ref move2);
        testI++;
        if (testI >= tests.GetLength(0))
            testI = 0;
        testFail = 0;
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
