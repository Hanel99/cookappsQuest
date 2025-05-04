
// 블럭 종류
public enum BlockType
{
    Normal,
    PengE,
    Count,
}


// 블럭 색상 구분
public enum BlockColor
{
    Blue,
    Green,
    Orange,
    Purple,
    Red,
    Yellow,
    Count,

}


// 노드 방향 확인
public enum Direction
{
    UP = 0,
    UPLEFT = 1,
    UPRIGHT = 2,
    DOWN = 3,
    DOWNLEFT = 4,
    DOWNRIGHT = 5,
    ERROR = 6, // 알수 없는 방향
}
