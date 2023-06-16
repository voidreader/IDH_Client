using UnityEngine;

class DamagePrint : MonoBehaviour
{
	UILabel text;
	UISprite sprite;

	Transform tf;
	Vector3 pos;
	float acc;
	float tgTime = 1f;
	float x;

	Vector3 worldPos;
	internal void Init(Vector3 _pos, int _damage, bool _left, UIFont _font, DamagePower _atkDp, DamagePower _grdDp, bool _over)
	{
		worldPos = _pos;
		pos = GameCore.Instance.WorldPosToUIPos(_pos);
		x = _left ? -30f : 30f;
		acc = 0;

		if (text == null)
			text = GetComponent<UILabel>();
		if (sprite == null)
			sprite = GetComponentInChildren<UISprite>();

		if (tf == null)
			tf = transform;

		text.bitmapFont = _font;

		tf.localPosition = pos;
		text.text = _damage.ToString("0");
		text.alpha = 1f;
        if (_over)
        {
            sprite.spriteName = "TEXT_OVERKILL";

            transform.localRotation = Quaternion.Euler(0f, 0f, 20f);
            sprite.transform.localRotation = Quaternion.Euler(0f, 0f, -20f);
            sprite.transform.localPosition = new Vector3(-100, 75, 0);
            sprite.alpha = 0.9f;
        }
        else
        {
            transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            sprite.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            sprite.transform.localPosition = new Vector3(-77, 35, 0);
            sprite.alpha = 1f;

            if (_atkDp == DamagePower.Critical)
			{
				sprite.spriteName = "TEXT_CRITICAL";
			}  
            else if (_grdDp == DamagePower.Critical)
			{
				sprite.spriteName = "TEXT_GUARD";
			} 
            else
			{
				// [NOTE] : 이전 코드에서 sprite.Name을 "" 처리하던 방식을 제거.
				// alpha값을 수정하는 방식으로 변경
				sprite.alpha = 0.0F;
			}
               
        }
		if(sprite.alpha != 0.0F)	
			sprite.MakePixelPerfect();

		gameObject.SetActive(true);
	}

	private void Update()
	{
		acc += Time.deltaTime * GameCore.timeScale;
		var value = acc / tgTime;

		if (value >= 1)
			gameObject.SetActive(false);
		else
		{
			var val = (value - 0.5f) * 2f;
			var y = 1 - val * val * val * val;
			tf.localPosition = GameCore.Instance.WorldPosToUIPos(worldPos) + (new Vector3(x * value, 90f * y, 0f));
            tf.localScale = value < 0.1f ? Vector3.Lerp(new Vector3(0.3f, 0.3f, 1f), Vector3.one, value / 0.1f) :
                            value < 0.6f ? Vector3.one :
                                           Vector3.Lerp(Vector3.one, new Vector3(0.3f, 0.3f, 1f), (value-0.6f) / 0.4f);
            text.alpha = value < 0.8f ? 1f : (0.2f - (value - 0.8f)) / 0.2f;
		}
	}

	//internal void OnFinishedTweening()
	//{
	//	gameObject.SetActive(false);
	//}
}
