﻿using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Controls the behavior of the character.
/// </summary>
public class HumanBehavior : MonoBehaviour
{
    // List of human actions
    public enum HumanActionType
    {
        Idling,
        Walking,
        Rotating,
        Turning,
        Waiting,
    };
    // Angle accuracy
    [Range(5f, 10f)]
    public float AngleAccuracy;
    // Checking distance during walking
    [Range(1f, 3f)]
    public float CheckingDistance;
    // Waiting time in way point
    [Range(1f, 3f)]
    public float WaitingTime;
    // Action time in way point
    [Range(2f, 5f)]
    public float ActionTime;
    // Rotation speed
    [Range(1f, 5f)]
    public float RotationSpeed;
    // Setting offset
    [Range(-0.05f, -0.01f)]
    public float SittingOffset;
    // Offset between collisions of agents
    [Range(0.1f, 0.5f)]
    public float CollisionOffset;
    // Gap between another speaking sentences
    [Range(1f, 2f)]
    public float SpeakingGap;
    // Destinations (way points)
    private Transform[] _destinations;
    // Agent offset
    private float _standardOffset;
    // Nav mesh agent
    private NavMeshAgent _agent;
    // Animator
    private Animator _animator;
    // Current look rotation
    private Quaternion _lookRotation;
    // Check if character is waliking
    private bool _isWalking;
    // Check if is right rotation
    private bool _isRotatingRight;
    // Check if is left rotation
    private bool _isRotatingLeft;
    // Check if character is turning
    private bool _isTurning;
    // Animator walking value
    private string _animWalk = "isWalking";
    // Animator rotating right value
    private string _animRotateRight = "isRotatingRight";
    // Animator rotating left value
    private string _animRotateLeft = "isRotatingLeft";
    // Animator turning value
    private string _animTurning = "isTurning";
    // Animator idle trigger
    private string _animIdle = "idle";
    // Animator reset trigger
    private string _animReset = "reset";
    // Current destination index
    private int _curDestination;
    // Current waiting time
    private float _curTime;
    // Translation time
    private float _translationTime;
    // Current action
    private HumanActionType _curAction;
    // Human types
    private Transform[] _humanTypes;
    // Navigation of people
    private Transform[] _navPoints;
    // Navigation point of this person
    private Transform _thisNavPoint;
    // Nav mesh path
    private NavMeshPath _path;
    // Gender
    private string _gender;

    // Awake is called when the script instance is being loaded
    private void Awake()
    {
        Init();
    }

    // Update is called once per frame
    private void Update()
    {
        SwitchHumanActions();
    }

    // Initializate parameters
    private void Init()
    {
        // Load eyes materials
        Material[] eyesMaterials = Resources.LoadAll<Material>("People/Materials/Eyes");
        // Get person gender
        _gender = name = name.Replace("(Clone)", "");
        // Get all objects in human
        Transform[] humanTransforms = transform.GetComponentsInChildren<Transform>();
        // Create temporary list
        List<Transform> humanTypes = new List<Transform>();
        // Get all human types
        foreach (Transform trans in humanTransforms)
        {
            // Check object tag
            if (trans.tag.Equals("HumanType"))
                // Add to list
                humanTypes.Add(trans);
        }
        // Transform list to array
        _humanTypes = humanTypes.ToArray();
        // Random human type
        int humanIndex = Random.Range(1, _humanTypes.Length + 1);
        // Random eye color
        int eyeIndex = Random.Range(0, eyesMaterials.Length);
        // Get eye renderer
        SkinnedMeshRenderer eyesRenderer = transform.Find(_gender + "Eyes")
            .GetComponent<SkinnedMeshRenderer>();
        // Set new eyes material
        eyesRenderer.material = eyesMaterials[eyeIndex];
        // Disable other types
        foreach (Transform trans in _humanTypes)
        {
            // Check human index
            if (trans.name.Contains("0" + humanIndex + "LOD"))
                // Go to next step
                continue;
            // Get mesh renderers
            SkinnedMeshRenderer[] skinnedMeshRenderers = trans
                .GetComponentsInChildren<SkinnedMeshRenderer>();
            // Search mesh renderers
            foreach (SkinnedMeshRenderer renderer in skinnedMeshRenderers)
                // Disable mesh renderer
                renderer.enabled = false;
        }
        // Man
        if (humanIndex.Equals(_humanTypes.Length) && _gender.Equals("Man"))
        {
            // Get body renderer
            SkinnedMeshRenderer bodyRenderer = transform.Find("ManBody")
                .GetComponent<SkinnedMeshRenderer>();
            // Set new eyes material (dark)
            eyesRenderer.material = eyesMaterials[eyesMaterials.Length - 1];
            // Set new body material (dark)
            bodyRenderer.material = Resources.Load<Material>("People/Man/Materials/ManBodyBlack");
        }
        // Woman
        if (_gender.Equals("Woman"))
        {
            // Set new skin and eyes (dark)
            if (Random.Range(0, 2).Equals(1))
            {
                // Get body renderer
                SkinnedMeshRenderer bodyRenderer = transform.Find("WomanBody")
                    .GetComponent<SkinnedMeshRenderer>();
                // Set new eyes material (dark)
                eyesRenderer.material = eyesMaterials[eyesMaterials.Length - 1];
                // Set new body material (dark)
                bodyRenderer.material = Resources.Load<Material>("People/Woman/Materials/WomanBodyDusky");
            }
            // Find fingernails
            SkinnedMeshRenderer skinnedMeshRenderer =
                transform.Find("WomanFingernails").GetComponent<SkinnedMeshRenderer>();
            // Set fingernails
            skinnedMeshRenderer.material =
                Resources.Load<Material>("People/Woman/Materials/Woman0" + humanIndex + "Fingernails");
            // Find high heels
            skinnedMeshRenderer = transform.Find("WomanHighHeels").GetComponent<SkinnedMeshRenderer>();
            // Set high heels
            skinnedMeshRenderer.material =
                Resources.Load<Material>("People/Woman/Materials/Woman0" + humanIndex + "HighHeels");
            // Find panty
            skinnedMeshRenderer = transform.Find("WomanPanty").GetComponent<SkinnedMeshRenderer>();
            // Set panty
            skinnedMeshRenderer.material =
                Resources.Load<Material>("People/Woman/Materials/Woman0" + humanIndex + "Panty");
        }
        // Set proper human area
        GameObject area = transform.parent.gameObject;
        // Get all area children
        Transform[] areaPoints = area.GetComponentsInChildren<Transform>();
        // Create temporary list
        List<Transform> wayPointsList = new List<Transform>();
        // Search way points
        foreach (Transform areaPoint in areaPoints)
        {
            // Check if it is way point
            if (areaPoint.name.Equals("Look Point") || areaPoint.name.Equals("Way Point"))
                // Set way point
                wayPointsList.Add(areaPoint);
        }
        // Transform list
        _destinations = wayPointsList.ToArray();
        _agent = gameObject.GetComponent<NavMeshAgent>();
        _animator = gameObject.GetComponent<Animator>();
        _isWalking = _isRotatingRight = _isRotatingLeft  = _isTurning = false;
        _curAction = HumanActionType.Idling;
        _curDestination = 0;
        _curTime = 0f;
        _standardOffset = _agent.baseOffset;
        _path = new NavMeshPath();
        _animator.SetBool(_animTurning, _isTurning);
        _animator.SetBool(_animWalk, _isWalking);
        _animator.SetBool(_animRotateRight, _isRotatingRight);
        _animator.SetBool(_animRotateLeft, _isRotatingLeft);
    }

    /// <summary>
    /// Sets the navigation points for the individual people.
    /// </summary>
    public void PreparePeople()
    {
        // Get all people
        GameObject[] people = GameObject.FindGameObjectsWithTag("Human");
        // Create temporary list of navigation points
        List<Transform> navPoints = new List<Transform>();
        // Search people
        foreach (GameObject person in people)
        {
            // Check person (compare parent of parent (regions))
            if (person.transform.parent.parent.name.Equals(transform.parent.parent.name)
                && !person.transform.parent.name.Equals(transform.parent.name))
                // Add navigation point to list
                navPoints.Add(person.transform.Find("Navigation"));
        }
        // Transform list
        _navPoints = navPoints.ToArray();
        // Set navigation point of this person
        _thisNavPoint = transform.Find("Navigation");
    }

    /// <summary>
    /// Stops the person in the specific position and waits.
    /// </summary>
    private void WaitAWhile()
    {
        // Reset auxiliary trigger
        _animator.ResetTrigger(_animReset);
        // Set trigger
        _animator.SetTrigger(_animIdle);
        // Check waiting time
        if (_curTime > WaitingTime)
        {
            // Reset trigger
            _animator.ResetTrigger(_animIdle);
            // Reset current time
            _curTime = 0f;
            // Set walking action
            _curAction = HumanActionType.Walking;
            // Break action
            return;
        }
        // Increase time
        _curTime += Time.deltaTime;
    }

    /// <summary>
    /// Navigates the person to the destination.
    /// </summary>
    private void GoToDestination()
    {
        // Set new path
        SetNewHumanPath(_curDestination);
        // Destination is reached (it is not way point!)
        if (_agent.remainingDistance <= _agent.stoppingDistance
            && !_destinations[_curDestination].name.Equals("Way Point"))
        {
            // Set animation
            _agent.isStopped = true;
            _isWalking = false;
            _animator.SetBool(_animWalk, _isWalking);
            // Set rotating action
            _curAction = HumanActionType.Rotating;
            // Break action
            return;
        }
        // Go to next way point
        if (_agent.remainingDistance < CheckingDistance
            && _destinations[_curDestination].name.Equals("Way Point"))
            // Set next human destination
            SetNextHumanDestination();
        // Set animation
        _agent.isStopped = false;
        _isWalking = true;
        _animator.SetBool(_animWalk, _isWalking);
    }

    /// <summary>
    /// Rotates the person to the specific object.
    /// </summary>
    private void RotateToDestination()
    {
        // Get child
        Transform destination = _destinations[_curDestination].GetChild(0);
        // Get look rotation
        _lookRotation = Quaternion.LookRotation(destination.position - transform.position);
        // Check if rotatin is completed
        if (Quaternion.Angle(transform.rotation, _lookRotation) < AngleAccuracy)
        {
            // Check destination type - rotation
            if (destination.name.Equals("Rotation Point"))
            {
                // Change state
                _curAction = HumanActionType.Idling;
                // Set new destination
                SetNextHumanDestination();
            }
            
            // Change values
            _isRotatingLeft = _isRotatingRight = false;
            // Set animation
            _animator.SetBool(_animRotateRight, _isRotatingRight);
            _animator.SetBool(_animRotateLeft, _isRotatingLeft);
            // Break action
            return;
        }
        // Check rotation direction
        _isRotatingRight = GetRotateDirection(transform.rotation, _lookRotation);
        _isRotatingLeft = !_isRotatingRight;
        // Set animation
        _animator.SetBool(_animRotateRight, _isRotatingRight);
        _animator.SetBool(_animRotateLeft, _isRotatingLeft);
        // Rotate only y axis
        _lookRotation.x = _lookRotation.z = 0f;
        // Rotate smoothly
        transform.rotation = Quaternion.Slerp(transform.rotation, _lookRotation,
            Time.deltaTime * RotationSpeed);
    }

    /// <summary>
    /// Gets the proper rotation direction.
    /// </summary>
    /// <param name="from">A structure that represents a start rotation.</param>
    /// <param name="to">A structure that represents an end rotation.</param>
    /// <returns>
    /// The boolean that is true if the rotation is clock wise or false if not.
    /// </returns>
    private bool GetRotateDirection(Quaternion from, Quaternion to)
    {
        // Character
        float fromY = from.eulerAngles.y;
        // Destination
        float toY = to.eulerAngles.y;
        // Left rotation
        float clockWise = 0f;
        // Right rotation
        float counterClockWise = 0f;
        // Check rotation direction
        if (fromY <= toY)
        {
            clockWise = toY - fromY;
            counterClockWise = fromY + (360f - toY);
        }
        else
        {
            clockWise = (360f - fromY) + toY;
            counterClockWise = fromY - toY;
        }
        // Return result
        return (clockWise <= counterClockWise);
    }

    /// <summary>
    /// Sets the turning animation while the person is sitting.
    /// </summary>
    private void TurnCharacter()
    {
        // Set animation
        _isTurning = true;
        _animator.SetBool(_animTurning, _isTurning);
    }


    /// <summary>
    /// Sets the next destination of the character.
    /// </summary>
    private void SetNextHumanDestination()
    {
        // Check current destination
        if (_curDestination.Equals(_destinations.Length - 1))
            // Reset path
            _curDestination = 0;
        // Set another destination
        else
            _curDestination++;
    }

    /// <summary>
    /// Sets the proper path for the person.
    /// </summary>
    /// <param name="destIndex">A number that represents the index of the route.</param>
    private void SetNewHumanPath(int destIndex)
    {
        // Calculate path
        _agent.CalculatePath(_destinations[destIndex].position, _path);
        // Assign new path
        _agent.SetPath(_path);
    }

    /// <summary>
    /// Resets the state of the person and sets start position.
    /// </summary>
    public void ResetPerson()
    {
        // Set standard offset
        _standardOffset = _agent.baseOffset;
        // Disable agent
        _agent.enabled = false;
        // Disable other animations
        _isWalking = _isRotatingRight = _isRotatingLeft  = _isTurning =
              false;
        _animator.SetBool(_animTurning, _isTurning);
        _animator.SetBool(_animWalk, _isWalking);
        _animator.SetBool(_animRotateRight, _isRotatingRight);
        _animator.SetBool(_animRotateLeft, _isRotatingLeft);
        // Reset time
        _curTime = 0f;
        // Reset animations
        _animator.SetTrigger(_animReset);
        // Reset person state
        _curAction = HumanActionType.Idling;
        // Set new destination
        _curDestination = 0;
        // Get start point
        Transform humanPoint = transform.parent.GetChild(0);
        // Change person position
        transform.position = humanPoint.position;
        // Change person rotation
        transform.rotation = humanPoint.rotation;
        // Enable agent
        _agent.enabled = true;
    }

 

    /// <summary>
    /// Switches the person actions during the simulation.
    /// </summary>
    private void SwitchHumanActions()
    {
        switch (_curAction)
        {
            case HumanActionType.Idling:
                WaitAWhile();
                break;
            case HumanActionType.Walking:
                GoToDestination();
                break;
            case HumanActionType.Rotating:
                RotateToDestination();
                break;
            case HumanActionType.Turning:
                TurnCharacter();
                break;

          
        }
    }
}