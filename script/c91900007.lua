-- Arcana Disguise Joker
local s,id=GetID()
function s.initial_effect(c)
	-- Fusion Material / Special Summon Procedure
	c:EnableReviveLimit()
	Fusion.AddProcMix(c,true,true,s.matfilter1,s.matfilter2,s.matfilter2)
	
	-- Contact Fusion (Must first be Special Summoned by banishing materials)
	local e0=Effect.CreateEffect(c)
	e0:SetType(EFFECT_TYPE_SINGLE)
	e0:SetProperty(EFFECT_FLAG_CANNOT_DISABLE+EFFECT_FLAG_UNCOPYABLE)
	e0:SetCode(EFFECT_SPSUMMON_CONDITION)
	e0:SetValue(aux.fuslimit)
	c:RegisterEffect(e0)

	local e0a=Effect.CreateEffect(c)
	e0a:SetType(EFFECT_TYPE_FIELD)
	e0a:SetProperty(EFFECT_FLAG_UNCOPYABLE)
	e0a:SetCode(EFFECT_SPSUMMON_PROC)
	e0a:SetRange(LOCATION_EXTRA)
	e0a:SetCondition(s.contactcon)
	e0a:SetTarget(s.contacttg)
	e0a:SetOperation(s.contactop)
	c:RegisterEffect(e0a)

	-- Effect 1: Send to GY to destroy 1 card
	local e1=Effect.CreateEffect(c)
	e1:SetDescription(aux.Stringid(id,0))
	e1:SetCategory(CATEGORY_TOGRAVE+CATEGORY_DESTROY)
	e1:SetType(EFFECT_TYPE_IGNITION)
	e1:SetRange(LOCATION_MZONE)
	e1:SetCountLimit(1,id)
	e1:SetTarget(s.destg)
	e1:SetOperation(s.desop)
	c:RegisterEffect(e1)

	-- Effect 2: Return to Extra Deck & Special Summon materials
	local e2=Effect.CreateEffect(c)
	e2:SetDescription(aux.Stringid(id,1))
	e2:SetCategory(CATEGORY_SPECIAL_SUMMON)
	e2:SetType(EFFECT_TYPE_IGNITION)
	e2:SetRange(LOCATION_MZONE)
	e2:SetCountLimit(1,{id,1})
	e2:SetCost(s.spcost)
	e2:SetTarget(s.sptg)
	e2:SetOperation(s.spop)
	c:RegisterEffect(e2)
end

-- Official Card IDs
local CARD_JOKERS_KNIGHT  = 29284413
local CARD_ARCANA_JOKER   = 6150044
local CARD_QUEENS_KNIGHT  = 25652259
local CARD_KINGS_KNIGHT   = 64788463
local CARD_JACKS_KNIGHT   = 90876561

-- Fusion Material Filters
function s.matfilter1(c,fc,sumtype,tp)
	return c:GetOriginalCode()==CARD_JOKERS_KNIGHT
end
function s.matfilter2(c,fc,sumtype,tp)
	return c:IsAttribute(ATTRIBUTE_LIGHT) and c:IsRace(RACE_WARRIOR)
end

-- /// Contact Fusion Procedures ///
function s.jkfilter(c,tp)
	return c:IsFaceup() and c:IsAbleToRemoveAsCost() and s.matfilter1(c)
end
function s.lwfilter(c,tp)
	return c:IsFaceup() and c:IsAbleToRemoveAsCost() and s.matfilter2(c)
end
function s.contactcon(e,c)
	if c==nil then return true end
	local tp=c:GetControler()
	local jkg=Duel.GetMatchingGroup(s.jkfilter,tp,LOCATION_MZONE,0,nil,tp)
	local lwg=Duel.GetMatchingGroup(s.lwfilter,tp,LOCATION_MZONE,0,nil,tp)
	if #jkg==0 then return false end
	-- need at least 2 Light Warriors excluding whichever card is used as the Joker's Knight material
	local jk=jkg:GetFirst()
	local remaining=lwg:Filter(function(c) return c~=jk end,nil)
	return #remaining>=2
end
function s.contacttg(e,tp,eg,ep,ev,re,r,rp,c)
	local jkg=Duel.GetMatchingGroup(s.jkfilter,tp,LOCATION_MZONE,0,nil,tp)
	if #jkg==0 then return false end
	Duel.Hint(HINT_SELECTMSG,tp,HINTMSG_REMOVE)
	local sjk=jkg:Select(tp,1,1,nil)
	local jk=sjk:GetFirst()

	local lwg=Duel.GetMatchingGroup(s.lwfilter,tp,LOCATION_MZONE,0,nil,tp)
	lwg:RemoveCard(jk)
	if #lwg<2 then return false end
	Duel.Hint(HINT_SELECTMSG,tp,HINTMSG_REMOVE)
	local slw=lwg:Select(tp,2,2,nil)

	local g=sjk
	g:Merge(slw)
	if #g==3 then
		g:KeepAlive()
		e:SetLabelObject(g)
		return true
	end
	return false
end
function s.contactop(e,tp,eg,ep,ev,re,r,rp,c)
	local g=e:GetLabelObject()
	if g then
		Duel.Remove(g,POS_FACEUP,REASON_COST+REASON_MATERIAL)
		g:Clear()
	end
end

-- /// Effect 1: Send Normal Monster or card mentioning "Queen's Knight" to GY to destroy ///
function s.tgfilter(c)
	return (c:IsType(TYPE_NORMAL) or c:ListsCode(CARD_QUEENS_KNIGHT)) and c:IsAbleToGrave()
end
function s.destg(e,tp,eg,ep,ev,re,r,rp,chk)
	if chk==0 then return Duel.IsExistingMatchingCard(s.tgfilter,tp,LOCATION_DECK,0,1,nil)
		and Duel.IsExistingMatchingCard(aux.TRUE,tp,LOCATION_ONFIELD,LOCATION_ONFIELD,1,e:GetHandler()) end
	Duel.SetOperationInfo(0,CATEGORY_TOGRAVE,nil,1,tp,LOCATION_DECK)
	Duel.SetOperationInfo(0,CATEGORY_DESTROY,nil,1,PLAYER_ALL,LOCATION_ONFIELD)
end
function s.desop(e,tp,eg,ep,ev,re,r,rp)
	Duel.Hint(HINT_SELECTMSG,tp,HINTMSG_TOGRAVE)
	local g=Duel.SelectMatchingCard(tp,s.tgfilter,tp,LOCATION_DECK,0,1,1,nil)
	if #g>0 and Duel.SendtoGrave(g,REASON_EFFECT)>0 and g:GetFirst():IsLocation(LOCATION_GRAVE) then
		Duel.Hint(HINT_SELECTMSG,tp,HINTMSG_DESTROY)
		local dg=Duel.SelectMatchingCard(tp,aux.TRUE,tp,LOCATION_ONFIELD,LOCATION_ONFIELD,1,1,nil)
		if #dg>0 then
			Duel.BreakEffect()
			Duel.Destroy(dg,REASON_EFFECT)
		end
	end
end

-- /// Effect 2: Return to Extra Deck and Special Summon Materials ///
function s.spcost(e,tp,eg,ep,ev,re,r,rp,chk)
	local c=e:GetHandler()
	if chk==0 then return c:IsAbleToExtra() end
	Duel.SendtoDeck(c,nil,SEQ_DECKTOP,REASON_COST)
end

function s.revealfilter(c)
	return c:IsCode(CARD_ARCANA_JOKER)
end

function s.sptg(e,tp,eg,ep,ev,re,r,rp,chk)
	local rg=Duel.GetMatchingGroup(s.revealfilter,tp,LOCATION_EXTRA,0,nil)
	if chk==0 then return #rg>0 and Duel.GetLocationCount(tp,LOCATION_MZONE)>0
		and Duel.IsExistingMatchingCard(aux.NecroValleyFilter(s.matfromzonefilter),tp,LOCATION_GRAVE+LOCATION_REMOVED,0,1,nil,e,tp) end
	Duel.SetOperationInfo(0,CATEGORY_SPECIAL_SUMMON,nil,1,tp,LOCATION_GRAVE+LOCATION_REMOVED)
end

function s.matfromzonefilter(c,e,tp)
	return (c:IsCode(CARD_QUEENS_KNIGHT) or c:IsCode(CARD_KINGS_KNIGHT) or c:IsCode(CARD_JACKS_KNIGHT))
		and c:IsCanBeSpecialSummoned(e,0,tp,false,false)
end

function s.spop(e,tp,eg,ep,ev,re,r,rp)
	Duel.Hint(HINT_SELECTMSG,tp,HINTMSG_CONFIRM)
	local rg=Duel.GetMatchingGroup(s.revealfilter,tp,LOCATION_EXTRA,0,nil)
	if #rg==0 then return end
	local rc=rg:Select(tp,1,1,nil):GetFirst()
	if not rc then return end
	Duel.ConfirmCards(1-tp,rc)

	local ft=Duel.GetLocationCount(tp,LOCATION_MZONE)
	if ft<=0 then return end

	local g=Duel.GetMatchingGroup(aux.NecroValleyFilter(s.matfromzonefilter),tp,LOCATION_GRAVE+LOCATION_REMOVED,0,nil,e,tp)
	if #g>0 then
		Duel.Hint(HINT_SELECTMSG,tp,HINTMSG_SPSUMMON)
		local sg=g:Select(tp,1,math.min(ft,3),nil)
		if #sg>0 then
			Duel.SpecialSummon(sg,0,tp,tp,false,false,POS_FACEUP)
		end
	end
end