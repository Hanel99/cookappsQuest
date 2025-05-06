
// 블럭 종류
public enum BlockType
{
    Normal,
    Special, //팽이 블럭
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
    ERROR = 6, // 알수 없는 방향 겸 카운트 용도로 사용
}

//게임 상태
public enum GameState
{
    Ready, // 게임 실행 준비 단계(초기 블럭 셋팅 등)
    Start, // 게임 실행 연출 단계
    GamePlay, // 실제 게임 진행 단계 
    GameClear, // 성공 조건 달성
    GameOver, // 패배 조건 달성
    Pause,
}