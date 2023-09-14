using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] private float _speed = 100.0f; //regular speed
    [SerializeField] private float _shiftAdd = 250.0f; //multiplied by how long shift is held.  Basically running
    [SerializeField] private float _maxShift = 1000.0f; //Maximum speed when holdin gshift
    [SerializeField] private float _camSens = 0.25f; //How sensitive it with mouse
    
    private Vector3 _lastMouse = new Vector3(255, 255, 255); //kind of in the middle of the screen, rather than at the top (play)
    private float _totalRun= 1.0f;

    private Transform _transform;

    private void Start()
    {
        _transform = transform;
    }

    private void Update () 
    {
        var eulerAngles = _transform.eulerAngles;

        _lastMouse = Input.mousePosition - _lastMouse ;
        _lastMouse = new Vector3(-_lastMouse.y * _camSens, _lastMouse.x * _camSens, 0 );
        _lastMouse = new Vector3(eulerAngles.x + _lastMouse.x , eulerAngles.y + _lastMouse.y, 0);
        eulerAngles = _lastMouse;
        _transform.eulerAngles = eulerAngles;
        _lastMouse =  Input.mousePosition;
       
        //Keyboard commands
        var input = GetBaseInput();
        
        if (Input.GetKey (KeyCode.LeftShift))
        {
            _totalRun += Time.deltaTime;
            input  = _shiftAdd * _totalRun * input;
            input.x = Mathf.Clamp(input.x, -_maxShift, _maxShift);
            input.y = Mathf.Clamp(input.y, -_maxShift, _maxShift);
            input.z = Mathf.Clamp(input.z, -_maxShift, _maxShift);
        }
        else
        {
            _totalRun = Mathf.Clamp(_totalRun * 0.5f, 1f, 1000f);
            input *= _speed;
        }
       
        input = input * Time.deltaTime;
        var newPosition = transform.position;
        
        if (Input.GetKey(KeyCode.Space))
        {
            var position = _transform.position;

            transform.Translate(input);
            newPosition.x = position.x;
            newPosition.z = position.z;
            position = newPosition;
            _transform.position = position;
        }
        else
        {
            transform.Translate(input);
        }
       
    }
     
    private Vector3 GetBaseInput() 
    {
        var pVelocity = new Vector3();
        if (Input.GetKey (KeyCode.W)){
            pVelocity += new Vector3(0, 0 , 1);
        }
        if (Input.GetKey (KeyCode.S)){
            pVelocity += new Vector3(0, 0, -1);
        }
        if (Input.GetKey (KeyCode.A)){
            pVelocity += new Vector3(-1, 0, 0);
        }
        if (Input.GetKey (KeyCode.D)){
            pVelocity += new Vector3(1, 0, 0);
        }
        return pVelocity;
    }
}
