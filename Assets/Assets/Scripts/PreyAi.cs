using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class PreyAi : MonoBehaviour
{
    //List of States and what they do
    //Patrol is the default state, this just allows the preyAI to move around until a gather spot is in range then it will start moving toward the spot
    //Gather makes the PreyAI sit on the gather spot for a set amount of time until it moves to the next spot, if it has surpassed the set amount of time
    //Run makes the PreyAi move towards a hiding spot, if it has not reached the hiding spot in 7 seconds it returns to patrol
    //Hide just makes the AI wait a small amount of time and teleports it to a Hutch Object then it will return to patrol
    public enum State
    {
        Patrol,
        Gather,
        Run,
        Hide
    }
    //Is the current state stored
    public State currentState;
    //NavAgent assigned to The AI Object
    private NavMeshAgent agent;
    //Bool that is used when the player gets to close
    bool playerNearby = false;
    //bool for when the AI is currently in hiding
    bool inHiding = false;
    //Time for how long Ai Gathers And Hides
    public float capWaitTime = 5;
    float waitTime = 0;
    //Time for how long it takes to return to Patrol state if the player is not nearby
    public float runCapTime = 7;
    float runTime = 0;
    //AI Speed
    public float preySpeed = 5;
    //Array for Locations the AI has to Gather From
    public Transform[] gatherPoints;
    //Array for locations the Ai can run to
    public Transform[] runPoints;
    //Teleport spot that the Ai will teleport to after hiding, simulates like tunneling
    public GameObject hutch;
    //current selected points
    private int currentRunPoint = 0;
    private int currentGatherPoint = 0;

    // Start is called before the first frame update
    void Start()
    {
        //Gets the NavMesh from the inspector
        agent = GetComponent<NavMeshAgent>();
        currentState = State.Patrol;
    }

    // Update is called once per frame
    void Update()
    {
        //idle(); Debug Function to AI Testing
        //Switchs, Patrol runs Patrol Function and detects if the player is nearby, if so switch to run
        //Gather,gather Function Runs, checks for player if nearby switch to Run
        //checks if player is not nearby, starts a timer, if in hiding set state to hide reset timer. if not hiding and timer passed switch to Patrol and reset timer
        //hide just runs the Hide function
        switch (currentState)
        {
            case State.Patrol:
                Patrol();
                if (playerNearby)
                {
                    currentState = State.Run;
                }
                break;
            case State.Gather:
                Gather();
                if (playerNearby)
                {
                    
                    currentState = State.Run;
                }
                break;
            case State.Run:
                Run();
                if (!playerNearby)
                {
                    runTime  += Time.deltaTime;
                    Debug.Log(runTime);
                    if (inHiding)
                    {
                        currentState = State.Hide;
                        runTime = 0;
                    }
                    else if (runTime >= runCapTime)
                    {
                        runTime = 0;
                        currentState = State.Patrol;
                    }
                    
                }
                break;
            case State.Hide:
                Hide();
                break;
        }
    }
    //This trigger is used to determine if the player has gotten to close to the PreyAI, if so it changes PlayerNearby to true.
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = true;
            Debug.Log("Player Entered");
        }
    }
    // if the player leaves the trigger it sets the bool to false
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
            Debug.Log("Player Left");
        }
    }

    void Patrol()
    {
        //makes the AI move to set Gatherpoints over a period of time
        if (gatherPoints.Length == 0) return;
        {
            transform.position = Vector3.MoveTowards
            (transform.position, gatherPoints[currentGatherPoint].position, preySpeed * Time.deltaTime);
        }

        agent.SetDestination(gatherPoints[currentGatherPoint].position);
        //when The preyAI reaches a point it  sets the state to gather
        if (Vector3.Distance(transform.position, gatherPoints[currentGatherPoint].position) < 0.5f)
        {
            currentState = State.Gather;
        }
    }
    //makes the AI wait for a set time before sending them back on patrol, also updates the Points by +1 so it will move to a new one everytime
    void Gather()
    {
        waitTime += Time.deltaTime;
        if (waitTime >= capWaitTime)
        {
            waitTime = 0;
            currentGatherPoint = (currentGatherPoint + 1) % gatherPoints.Length;
            currentState = State.Patrol;
        }
    }
    // makes the Ai move toward the Hide points, if it reaches one activate bool for hiding
        void Run()
    {
        if (runPoints.Length == 0) return;
        {
            transform.position = Vector3.MoveTowards
            (transform.position, runPoints[currentRunPoint].position, preySpeed * Time.deltaTime);
        }
        agent.SetDestination(runPoints[currentRunPoint].position);
        if (Vector3.Distance(transform.position, runPoints[currentRunPoint].position) < 0.5f)
        {
            inHiding = true;
            
        }
    }
    //teleport Ai to the hutch, start a timer, when done reset time, increase hide point by one change to patrol state, set hiding bool to false
    void Hide()
    {
        transform.position = hutch.transform.position;
        waitTime += Time.deltaTime;
        if (waitTime >= capWaitTime)
        {
            waitTime = 0;
            currentRunPoint = (currentRunPoint + 1) % runPoints.Length;
            currentState = State.Patrol;
            inHiding = false;
        }
    }
    //Debug Function to test AI Movement
    //void idle()
    //{
    //    if (points.Length == 0) return;

    //    // Move toward current point
    //    transform.position = Vector3.MoveTowards(
    //        transform.position,
    //        points[currentPoint].position,
    //        preySpeed * Time.deltaTime
    //    );

    //    // If close enough, go to next point
    //    if (Vector3.Distance(transform.position, points[currentPoint].position) < 0.5f)
    //    {
    //        currentPoint = (currentPoint + 1) % points.Length;
    //    }
}
