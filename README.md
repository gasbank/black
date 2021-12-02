# 블랙 프로젝트 계획

누군가 심취한 OKR 기법을 본 게임 프로젝트에 적용해보자.

## Objective
- 2021년 12월 31일 (금) Google Play 출시

## Key Results
- 1주차: 개발 계획 논의 및 확정, 기존 기억 되살리기, 플레이 빌드 만들기
- 2주차
- 3주차
- 4주차
- 5주차

## 개발 요소

### 큰 방향
- 색칠하기 그 자체에 모든 것을 집중하고, 부가적인 요소는 모두 배제
- 스테이지: 100개
    - 튜토리얼 및 일반 스테이지 90개
    - 관문 스테이지 10개
    - 일정 여의치 않는 경우 스테이지는 위 수량의 절반 분량으로도 출시 가능

### 상세

1. 튜토리얼 스테이지
    - 뻔한 게임성이지만 태어나서 이런 게임을 처음 하는 사람 기준으로 간결하게 설명하는 기능 (초밥사천성 튜토리얼 수준)
    - 첫 설치 신규 유저라면 모두 이것으로 시작
    - 스킵 불가

2. 색칠하기 기능 자체
    - 거의 완료
    - 비주얼적인 개선 여지 있으나 이건 맨 나중으로 미룸

2. 색칠하기 입장 (스테이지 시작) 연출
    - '두둥~'류 사운드와 그에 맞는 UI

3. 색칠하기 잘못된 액션 알림
    - 일반 스테이지에서는 페널티 없고 알려주는 용도만 (전체 화면 빨간색 번쩍)
    - 관문 스테이지에서는 시간 페널티

4. 관문 스테이지
    - 5의 배수 스테이지에서만 관문 스테이지 등장
    - 시간제한 있고, 페널티 시 시간 깎임
    - 한주님의 그림 사용
    - 관문 스테이지는 그에 걸맞는 스테이지명 지정 (리더보드 항목에 사용)

5. 일반 스테이지
    - 인터넷에서 퍼와서 사용 (라이선스 문제 없는 것으로)
    - 이런 이미지가 얼마나 많을 지 모르겠으므로 조사 필요
    - 일반 스테이지는 특별한 이름 없음 (스테이지 15, 스테이지 16, ...)

6. 리더보드
    - 구글/애플 리더보드 기능 그대로 활용
    - "클리어한 스테이지": 최종적으로 깬 스테이지 몇 인지 기록
    - 관문 스테이지 별 최단 시간 기록 경쟁
    
7. 업적
    - 구글/애플 업적 기능 그대로 활용
    - 처음 스테이지 5개까지는 매번 주고, 이후부터는 관문 스테이지마다
    - 그럴듯한 재밌는 이름 붙이기 ('색을 칠해보자', '능숙해지는 느낌', '참 잘했어요', ...)

8. 스테이지 선택 화면
    - 이미 클리어한 스테이지는 맘대로 선택해서 플레이 가능
    - 관문 스테이지의 경우 재플레이 때마다 기록 갱신 됨
        (기존 기록 갱신 시 축하 UI)

9. 힌트 기능
    - 힌트 기능은 어느 정도 어려워졌을 때부터 처음 등장 (스테이지 6부터?)
    - 색칠할 칸('셀') 기준 50% 넘게 색칠한 이후부터 힌트 기능 활성화
    - 힌트 기능 사용 시 힌트 쿠폰 하나 차감
    - 힌트 사용했을 때 알려주는 셀은 색칠되지 않은 칸 중 랜덤 (힌트로 알려준 칸을 다시 알려줄 수도 있음)

10. 힌트 쿠폰 획득 방법
    - 처음에 5개 무료로 주기
    - 관문 스테이지 클리어 시마다 5개 주기
    - 보상 광고 시청 1회에 힌트 쿠폰 5개 받음
        (힌트 기능 활성화가 된 이후에는 언제든지 시청 가능)

11. 게임 상태 저장
    - 게임 어느 시점에서 종료되어도, 마지막 상태로 복원되어야 함
