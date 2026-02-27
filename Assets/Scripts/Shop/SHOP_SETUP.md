# 상점 시스템 설정 가이드

## 1. 샘플 데이터 생성
Unity 메뉴: **Tools → Create Shop Sample Data** 실행
- 새 아이템 (토마토, 당근, 나무, 돌, 빵) 생성
- Shop_General.asset 상점 데이터 생성
- ItemDatabase에 새 아이템 자동 등록

## 2. 씬 설정

### ShopManager
- 빈 GameObject 생성 → 이름 "ShopManager"
- `ShopManager` 컴포넌트 추가

### ShopUI
- Canvas 하위에 Shop 패널 생성
- `ShopUI` 컴포넌트 추가 후 다음 연결:

| 필드 | 설명 |
|------|------|
| shopPanel | 상점 전체 패널 |
| txtShopName | 상점 이름 텍스트 |
| btnClose | 닫기 버튼 (CloseBtn). Inspector에서 OnClick에 ShopUI.OnCloseClick 수동 연결 가능 |
| btnBuy | 구매 버튼 (BuyBtn) |
| btnSell | 판매 버튼 (CellBtn) |
| slotContent | 슬롯 그리드 부모 (Grid Layout Group 등) |
| slotPrefab | 슬롯 프리팹 |
| txtGold | 골드 표시 (GoldTxt/Gold) |
| txtMessage | 알림 메시지 (선택) |
| debugShopData | 패널 수동 활성화 테스트 시 사용할 ShopData (선택) |

### 슬롯 프리팹 필수
- **Slot 루트**에 Image (Raycast Target 체크) 또는 빈 슬롯이면 코드가 자동 추가
- **Icon** 경로: `Icon`, `Item/Icon`, `Item` 중 하나
- 자식 Image의 Raycast Target을 끄면 슬롯 루트가 툴팁/클릭을 받음

### 판매 모드
- 같은 아이템 여러 개 = **1개 슬롯** (종류별로 1개씩만 표시)
- 예: 딸기 3개 보유 → 딸기 슬롯 1개

### CloseBtn이 안 될 때
- Inspector에서 CloseBtn의 OnClick에 ShopUI → OnCloseClick 수동 연결

### 목록이 안 뜰 때 체크
1. **slotContent**, **slotPrefab** 할당 여부
2. **ShopManager**가 씬에 있는지
3. **ShopInteract**로 열 때: **shopData** 할당 여부
4. 패널만 켜서 테스트: **debugShopData**에 Shop_General 할당
5. **ItemTooltip**이 씬에 있고 Canvas 하위인지

### 슬롯 프리팹 구조 (slotPrefab)
- 좌클릭: 1개 구매/판매, 우클릭: 5개 구매/판매
- 마우스 오버 시 툴팁 (아이템명, 종류, 설명, 구매가, 판매가)
- **클릭 감지를 위해** Slot 루트에 Image(투명, Raycast Target 체크) 또는 ShopSlotUI 추가

```
Slot (Image - Raycast Target 체크)
├── Icon (Image) - 또는 Item/Icon
└── Count (TMP_Text) - 판매 모드에서는 숨김
```

### ItemTooltip 가격 표시
- 툴팁에 `txtBuyPrice`, `txtSellPrice` 추가 시 구매/판매 가격 표시
- 인벤토리와 상점 동일 툴팁 사용

### 상점 열기
- 상점 NPC/건물에 `ShopInteract` 컴포넌트 추가
- `shopData`에 Shop_General (또는 다른 ShopData) 할당
- 플레이어가 클릭 또는 F키로 상점 열기

## 3. 아이템 가격 설정
ItemData/SeedData/ToolData의 `buyPrice`, `sellPrice` 필드:
- buyPrice: 상점 구매 가격 (0 = 구매 불가)
- sellPrice: 상점 판매 가격 (0 = 판매 불가)
