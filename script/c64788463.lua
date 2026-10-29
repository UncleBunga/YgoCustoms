-- House rule (no missing the timing): King's Knight.
-- Official script: an optional "When this card is Normal Summoned" trigger. When King's Knight is Normal Summoned by a
-- card effect inside a chain (Joker's Wild resolving as Joker's Straight), EDOPro holds the summon until the chain's
-- cleanup, where no trigger can start, so it misses the timing. Here a summon made by a card effect offers the
-- Special Summon straight away (it does not start a chain); a normal Normal Summon keeps the official trigger.
--キングス・ナイト
--King's Knight
local s,id=GetID()
function s.initial_effect(c)
	--spsummon (normal Normal Summon)
	local e1=Effect.CreateEffect(c)
	e1:SetDescription(aux.Stringid(id,0))
	e1:SetCategory(CATEGORY_SPECIAL_SUMMON)
	e1:SetType(EFFECT_TYPE_SINGLE+EFFECT_TYPE_TRIGGER_O)
	e1:SetProperty(EFFECT_FLAG_DELAY)
	e1:SetCode(EVENT_SUMMON_SUCCESS)
	e1:SetCondition(function(e,tp,eg,ep,ev,re,r,rp) return not re and s.spcon(e,tp,eg,ep,ev,re,r,rp) end)
	e1:SetTarget(s.sptg)
	e1:SetOperation(s.spop)
	c:RegisterEffect(e1)
	--spsummon (Normal Summoned by a card effect)
	local e2=Effect.CreateEffect(c)
	e2:SetType(EFFECT_TYPE_SINGLE+EFFECT_TYPE_CONTINUOUS)
	e2:SetCode(EVENT_SUMMON_SUCCESS)
	e2:SetCondition(function(e,tp,eg,ep,ev,re,r,rp) return re~=nil and s.spcon(e,tp,eg,ep,ev,re,r,rp) end)
	e2:SetOperation(s.effop)
	c:RegisterEffect(e2)
end
s.listed_names={CARD_QUEEN_KNIGHT,CARD_JACK_KNIGHT}
function s.cfilter(c)
	return c:IsFaceup() and c:IsCode(CARD_QUEEN_KNIGHT)
end
function s.spcon(e,tp,eg,ep,ev,re,r,rp)
	return Duel.IsExistingMatchingCard(s.cfilter,tp,LOCATION_ONFIELD,0,1,nil)
end
function s.filter(c,e,tp)
	return c:IsCode(CARD_JACK_KNIGHT) and c:IsCanBeSpecialSummoned(e,0,tp,false,false)
end
function s.sptg(e,tp,eg,ep,ev,re,r,rp,chk)
	if chk==0 then return Duel.GetLocationCount(tp,LOCATION_MZONE)>0
		and Duel.IsExistingMatchingCard(s.filter,tp,LOCATION_DECK,0,1,nil,e,tp) end
	Duel.SetOperationInfo(0,CATEGORY_SPECIAL_SUMMON,nil,1,tp,LOCATION_DECK)
end
function s.spop(e,tp,eg,ep,ev,re,r,rp)
	if Duel.GetLocationCount(tp,LOCATION_MZONE)<=0 then return end
	Duel.Hint(HINT_SELECTMSG,tp,HINTMSG_SPSUMMON)
	local g=Duel.SelectMatchingCard(tp,s.filter,tp,LOCATION_DECK,0,1,1,nil,e,tp)
	if #g>0 then
		Duel.SpecialSummon(g,0,tp,tp,false,false,POS_FACEUP)
	end
end
function s.effop(e,tp,eg,ep,ev,re,r,rp)
	local c=e:GetHandler()
	if Duel.GetLocationCount(tp,LOCATION_MZONE)<=0 or not Duel.IsExistingMatchingCard(s.filter,tp,LOCATION_DECK,0,1,nil,e,tp) then return end
	if not Duel.SelectEffectYesNo(tp,c,aux.Stringid(id,0)) then return end
	Duel.Hint(HINT_CARD,0,id)
	s.spop(e,tp,eg,ep,ev,re,r,rp)
end
