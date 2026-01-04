using UnityEngine;

public class CustomRotator : MonoBehaviour
{
    public enum Axis { X, Y, Z }
    public Axis rotationAxis = Axis.Y;

    public float rotationSpeed = 10f;
    public bool continuousRotation = true;
    public float maxRotationAngle = 45f;
    public float pauseDuration = 1f;

    private float currentAngle = 0f;
    private float direction = 1f;
    private bool isPaused = false;
    private float pauseTimer = 0f;
    private Vector3 rotationAxisVector;

    private void Start()
    {
        rotationAxisVector = rotationAxis switch
        {
            Axis.X => Vector3.right,
            Axis.Y => Vector3.up,
            Axis.Z => Vector3.forward,
            _ => Vector3.up
        };
    }

    void Update()
    {
        if (continuousRotation)
        {
            float angle = rotationSpeed * Time.deltaTime;
            transform.Rotate(rotationAxisVector, angle);
        }
        else
        {
            if (!isPaused)
            {
                float angle = rotationSpeed * Time.deltaTime * direction;
                transform.Rotate(rotationAxisVector, angle);
                currentAngle += angle;

                if (Mathf.Abs(currentAngle) >= maxRotationAngle)
                {
                    direction *= -1f;
                    currentAngle = Mathf.Clamp(currentAngle, -maxRotationAngle, maxRotationAngle);
                    isPaused = true;
                    pauseTimer = 0f;
                }
            }
            else
            {
                pauseTimer += Time.deltaTime;
                if (pauseTimer >= pauseDuration)
                {
                    isPaused = false;
                }
            }
        }
    }
}
