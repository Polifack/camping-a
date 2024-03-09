using System.Collections.Generic;

public class PlayerStateFactory
{
    PlayerStateMachine _context;
    Dictionary<string, PlayerBaseState> _states = new Dictionary<string, PlayerBaseState>();

    public PlayerStateFactory(PlayerStateMachine currentContext)
    {
        _context = currentContext;
        _states["idle"] = new PlayerIdleState(_context, this);
        _states["run"] = new PlayerRunState(_context, this);
        _states["jump"] = new PlayerJumpState(_context, this);
        _states["fall"] = new PlayerFallState(_context, this);
        _states["grounded"] = new PlayerGroundedState(_context, this);
        _states["ledgegrab"] = new PlayerLedgeGrabState(_context, this);
    }

    public PlayerBaseState Idle(){
        return _states["idle"];
    }
    public PlayerBaseState Run(){
        return _states["run"];
    }
    public PlayerBaseState Jump(){
        return _states["jump"];
    }
    public PlayerBaseState Grounded(){
        return _states["grounded"];
    }
    public PlayerBaseState Fall(){
        return _states["fall"];
    }
    public PlayerBaseState LedgeGrab(){
        return _states["ledgegrab"];
    }
}