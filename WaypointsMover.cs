using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Smart2DWaypoints
{
    public class WaypointsMover : MonoBehaviour
    {
        protected Transform Transform;

        private int _previousTargetIndex;
        protected int _targetIndex;
        private bool _isRunning;
        private bool _isForwardDirection;
        private Vector2 _startScale;
        private int _passedWaypointsCount;
        private float _pathPartStartTime;
        private float _pathPartAllTime;
        private float _nextPartTime;
        private bool _isPaused;
        private float _pauseTime;

        public Path Path;
        public Transform StartWaypoint;
        public LoopType LoopType = LoopType.Looped;
        public int Loops;
        public bool IsDestroyWhenCompleted;
        public bool IsAlignToDirection;
        public float RotationOffset;
        public bool IsXFlipEnabled;
        public bool IsYFlipEnabled;
        public Vector3 Offset;

        public const float InitSpeed = 0.139139f;

        [FormerlySerializedAs("Speed")] [SerializeField]
        private float _speed = InitSpeed;

        private int _cycles = 0;
        private float _currentTime;

        public float Speed
        {
            get => _speed;
            set
            {
                UpdateSpeed(_speed, value);
                _speed = value;
            }
        }

        public bool IsForwardDirection => _isForwardDirection;

        public float CurrentTime => _currentTime;

        [NonSerialized] public Vector3 Direction;

        public UnityAction OnCompleted;
        public UnityAction OnUpdated;
       

        public virtual void Awake()
        {
            Transform = transform;
            _startScale = Transform.localScale;
            _isForwardDirection = true;

            if (Path == null)
            {
                _isRunning = false;
            }
            else
            {
                Run();
            }
        }
        
        

        #region Running

        private void Run()
        {
            StartRunning();
            _targetIndex = Path.GetIndex(StartWaypoint);
            Transform.position = Path.CapturePosition(this, _targetIndex)+Offset;
            StartCoroutine(OnWaypointReached());
        }

        public virtual void FixedUpdate()
        {
            OnUpdate();
        }

        protected void OnUpdate()
        {
            if (_isRunning && !_isPaused)
            {
                UpdateMovement();
            }

            if (_isRunning && !_isPaused)
            {
                AlignToDirection();
                UpdateFlips();
            }
        }

        private void InitializeStartDirection()
        {
            if (_passedWaypointsCount == 1 && IsAlignToDirection)
            {
                Vector3 point = Path.GetPoint(this, 0.01f)+Offset;
                Direction = (point - Transform.position).normalized;
                AlignToDirection();
            }
        }

        private void AlignToDirection()
        {
            if (IsAlignToDirection && Direction != Vector3.zero)
            {
                float angle = Direction.y > 0
                    ? Vector2.Angle(new Vector2(1, 0), Direction)
                    : -Vector2.Angle(new Vector2(1, 0), Direction);
                SetRotationAngle(angle + RotationOffset);
            }
        }

        protected virtual void SetRotationAngle(float angle)
        {
            Transform.rotation = Quaternion.identity;
            Transform.Rotate(Vector3.forward, angle);
        }

        protected bool TakeNextWaypoint()
        {
            switch (LoopType)
            {
                case LoopType.Looped:
                {
                    if (Path.IsClosed && IsAtFirstWaypoint()&& _passedWaypointsCount > 0 || IsAtLastWaypoint() ) //for not closed path jump immediately to first point
                    {
                        this._cycles++;
                        if (this.Loops > 0 && this._cycles >= this.Loops)
                        {
                            StopRunning();
                            OnCompleted?.Invoke();
                            if (IsDestroyWhenCompleted)
                            {
                                Destroy(gameObject);
                            }
                            return true;
                        }
                        Transform.position = Path.Waypoints[0].position+Offset;
                    }
                    break;
                }

                case LoopType.PingPong:
                {
                    if ((IsAtFirstWaypoint() &&
                         (!_isForwardDirection || (_isForwardDirection && _passedWaypointsCount > 0))) ||
                        (IsAtLastWaypoint() && _isForwardDirection))
                    {
                        this._cycles++;
                        if (this.Loops > 0 && this._cycles >= this.Loops*2)
                        {
                            StopRunning();
                            OnCompleted?.Invoke();
                            if (IsDestroyWhenCompleted)
                            {
                                Destroy(gameObject);
                            }
                            return true;
                        }
                        _isForwardDirection = !_isForwardDirection;
                    }
                    break;
                }
            }
            MoveTargetIndexToNext();
            return false;
        }

        private bool IsAtFirstWaypoint()
        {
            return _targetIndex == 0;
        }

        private bool IsAtLastWaypoint()
        {
            return !Path.IsClosed && _targetIndex == Path.WaypointsCount - 1;
        }

        private void MoveTargetIndexToNext()
        {
            _previousTargetIndex = _targetIndex;
            int dInd = _isForwardDirection ? 1 : -1;
            _targetIndex = (_targetIndex + dInd + Path.WaypointsCount) % Path.WaypointsCount;
            _passedWaypointsCount++;
        }

        protected virtual void StopRunning()
        {
            _isRunning = false;
        }

        protected virtual void StartRunning()
        {
            _isRunning = true;
        }

        protected IEnumerator OnWaypointReached()
        {
            if (Path.HasDelay(_targetIndex))
            {
                StopRunning();
                yield return new WaitForSeconds(Path.GetDelay(_targetIndex));
                StartRunning();
            }

            if (TakeNextWaypoint()) yield break;
            try
            {
                Path.StartNextPart(this, _previousTargetIndex, _targetIndex, _isForwardDirection);
                _pathPartAllTime = Path.GetLength(this, _previousTargetIndex, _targetIndex, _isForwardDirection) / Speed;
                _pathPartStartTime = Time.fixedTime - _nextPartTime;
                _nextPartTime = 0;

                UpdateMovement();

                if (!Path.IsClosed && LoopType == LoopType.Looped && _targetIndex == 0)
                    StartCoroutine(OnWaypointReached());

                InitializeStartDirection();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void UpdateMovement()
        {
            float pathPartTime = (Time.fixedTime - _pathPartStartTime) / _pathPartAllTime;
            if (pathPartTime > 1f)
            {
                UpdatePoint(1f);
                _nextPartTime = (pathPartTime - 1f) * _pathPartAllTime;
                StartCoroutine(OnWaypointReached());
            }
            else
            {
                UpdatePoint(pathPartTime);
            }
        }

        private void UpdatePoint(float pathPartTime)
        {
            try
            {
                Vector3 point = Path.GetPoint(this, pathPartTime)+Offset;
                Direction = (point - Transform.position).normalized;
                UpdatePoint(point);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        protected virtual void UpdatePoint(Vector3 point)
        {
            Transform.position = new Vector3(point.x, point.y, point.z+Offset.z);
            OnUpdated?.Invoke();
        }

        #endregion

        #region Flips

        private void UpdateFlips()
        {
            var rotation = Transform.rotation;
            var scale = Transform.localScale;
            if(scale.x > 0 && rotation.z < 5)
            if (IsXFlipEnabled && Transform.localScale.x*Direction.x*Mathf.Sign(_startScale.x) < 0)
                Transform.localScale = new Vector2(-Transform.localScale.x, Transform.localScale.y);
            
            if (IsYFlipEnabled && Transform.localScale.y*Direction.y*Mathf.Sign(_startScale.y) < 0)
                Transform.localScale = new Vector3(Transform.localScale.x, -Transform.localScale.y);
            RotationOffset = transform.localScale.x < 0 ? 180f : 0f;
        }

        #endregion

        #region Gizmos

        public void OnDrawGizmos()
        {
            if (Camera.main)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(transform.position, transform.position + Direction * Camera.main.orthographicSize / 2f);
            }
        }

        #endregion

        #region Public API

        public void Go(Path path, float nextPartTime = 0, int startWaypointIndex = 0)
        {
            this._nextPartTime = nextPartTime;
            Path = path;
            StartWaypoint = path.Waypoints[startWaypointIndex];
            Run();
        }

        public void Go(Path path, Transform startWaypoint)
        {
            int index = path.Waypoints.IndexOf(startWaypoint);
            Go(path, index == -1 ? 0 : index);
        }

        #endregion

        #region Pause

        public void Pause()
        {
            if (!_isPaused)
            {
                _pauseTime = Time.fixedTime;
                _isPaused = true;
                OnPause();
            }
        }

        protected virtual void OnPause()
        {
        }

        public void Resume()
        {
            if (_isPaused)
            {
                _isPaused = false;
                _pathPartStartTime += Time.fixedTime - _pauseTime;
                OnResume();
            }
        }

        protected virtual void OnResume()
        {
        }

        public bool IsPaused()
        {
            return _isPaused;
        }

        #endregion

        private void UpdateSpeed(float oldSpeed, float newSpeed)
        {
            float st2 = _pathPartStartTime + (Time.fixedTime - _pathPartStartTime) * (1 - oldSpeed / newSpeed);
            _pathPartAllTime = (Time.fixedTime - st2) / ((Time.fixedTime - _pathPartStartTime) / _pathPartAllTime);
            _pathPartStartTime = st2;
        }
    }
}