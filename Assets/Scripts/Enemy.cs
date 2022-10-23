public class Enemy : PeopleAI
{
    protected override void Start()
    {
        MoveState = MoveState.Chase;
    }
}