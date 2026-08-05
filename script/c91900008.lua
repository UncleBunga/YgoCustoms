-- A-to-Z Dragon Buster Catapult Cannon
local s,id=GetID()
function s.initial_effect(c)
	-- Fusion Material / Special Summon Procedure
	c:EnableReviveLimit()
	Fusion.AddProcMix(c,true,true,s.matfilter1,s.matfilter2)
	
	-- Contact Fusion (Must first be Special Summoned by banishing the above cards)
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

	-- Effect 1: Quick Effect Negate (Activation or Summon)
	local e1=Effect.CreateEffect(c)
	e1:SetDescription(aux.Stringid(id,0))
	e1:SetCategory(CATEGORY_NEGATE+CATEGORY_DESTROY)
	e1:SetType(EFFECT_TYPE_QUICK_O)
	e1:SetCode(EVENT_CHAINING)
	e1:SetRange(LOCATION_MZONE)
	e1:SetCost(s.negcost)
	e1:SetTarget(s.negtg)
	e1:SetOperation(s.negop)
	c:RegisterEffect(e1)

	local e1b=Effect.CreateEffect(c)
	e1b:SetDescription(aux.Stringid(id,0))
	e1b:SetCategory(CATEGORY_DISABLE_SUMMON+CATEGORY_DESTROY)
	e1b:SetType(EFFECT_TYPE_QUICK_O)
	e1b:SetCode(EVENT_SUMMON)
	e1b:SetRange(LOCATION_MZONE)
	e1b:SetCost(s.negcost)
	e1b:SetTarget(s.negtg)
	e1b:SetOperation(s.negop)
	c:RegisterEffect(e1b)

	local e1c=e1b:Clone()
	e1c:SetCode(EVENT_SPSUMMON)
	c:RegisterEffect(e1c)

	-- Effect 2: Change battle positions
	local e2=Effect.CreateEffect(c)
	e2:SetDescription(aux.Stringid(id,1))
	e2:SetCategory(CATEGORY_POSITION)
	e2:SetType(EFFECT_TYPE_IGNITION)
	e2:SetRange(LOCATION_MZONE)
	e2:SetCountLimit(1)
	e2:SetTarget(s.postg)
	e2:SetOperation(s.posop)
	c:RegisterEffect(e2)

	-- Effect 3: Special Summon banished Machine monsters when leaving field
	local e3=Effect.CreateEffect(c)
	e3:SetDescription(aux.Stringid(id,2))
	e3:SetCategory(CATEGORY_SPECIAL_SUMMON)
	e3:SetType(EFFECT_TYPE_SINGLE+EFFECT_TYPE_TRIGGER_O)
	e3:SetProperty(EFFECT_FLAG_DELAY)
	e3:SetCode(EVENT_LEAVE_FIELD)
	e3:SetCondition(s.spcon)
	e3:SetTarget(s.sptg)
	e3:SetOperation(s.spop)
	c:RegisterEffect(e3)
end

-- Official Card IDs / Code Aliases for Materials
local CARD_ATOZ_DRAGON_BUSTER   = 58249829
local CARD_VWTIGER_CATAPULT   = 35699700
local CARD_ABC_DRAGON_BUSTER  = 95286165
local CARD_VWXYZ_DRAGON       = 84283223

-- Fusion Material Filters (Supports either listed pairing)
function s.matfilter1(c,fc,sumtype,tp)
	return c:IsCode(CARD_ATOZ_DRAGON_BUSTER) or c:IsCode(CARD_VWTIGER_CATAPULT) or c:IsCode(CARD_ABC_DRAGON_BUSTER)
end
function s.matfilter2(c,fc,sumtype,tp)
	return c:IsCode(CARD_VWXYZ_DRAGON)
end

-- /// Contact Fusion Procedures ///
function s.contactfilter(c,tp)
	return c:IsFaceup() and c:IsAbleToRemoveAsCost() and (s.matfilter1(c,nil,SUMMON_TYPE_SPECIAL,tp) or s.matfilter2(c,nil,SUMMON_TYPE_SPECIAL,tp))
end
function s.contactcon(e,c)
	if c==nil then return true end
	local tp=c:GetControler()
	local rg=Duel.GetMatchingGroup(s.contactfilter,tp,LOCATION_MZONE,0,nil,tp)
	return Duel.GetLocationCount(tp,LOCATION_MZONE)>-2
		and aux.SelectUnselectGroup(rg,e,tp,2,2,aux.dabcheck,0)
end
function s.contacttg(e,tp,eg,ep,ev,re,r,rp,c)
	local rg=Duel.GetMatchingGroup(s.contactfilter,tp,LOCATION_MZONE,0,nil,tp)
	local g=aux.SelectUnselectGroup(rg,e,tp,2,2,aux.dabcheck,1,tp,HINTMSG_REMOVE,nil,nil,true)
	if #g>0 then
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

-- /// Effect 1: Negate Activation / Summon ///
function s.negcost(e,tp,eg,ep,ev,re,r,rp,chk)
	if chk==0 then return Duel.IsExistingMatchingCard(Card.IsDiscardable,tp,LOCATION_HAND,0,1,e:GetHandler()) end
	Duel.DiscardHand(tp,Card.IsDiscardable,1,1,REASON_COST+REASON_DISCARD)
end
function s.negtg(e,tp,eg,ep,ev,re,r,rp,chk)
	local isSummon=(ev==EVENT_SUMMON or ev==EVENT_SPSUMMON or ev==EVENT_FLIP_SUMMON)
	if chk==0 then return true end
	if isSummon then
		Duel.SetOperationInfo(0,CATEGORY_DISABLE_SUMMON,eg,#eg,0,0)
	else
		Duel.SetOperationInfo(0,CATEGORY_NEGATE,eg,1,0,0)
	end
	if eg:GetFirst():IsRelateToEffect(re) or isSummon then
		Duel.SetOperationInfo(0,CATEGORY_DESTROY,eg,1,0,0)
	end
end
function s.negop(e,tp,eg,ep,ev,re,r,rp)
	local isSummon=(ev==EVENT_SUMMON or ev==EVENT_SPSUMMON or ev==EVENT_FLIP_SUMMON)
	if isSummon then
		Duel.NegateSummon(eg)
		Duel.Destroy(eg,REASON_EFFECT)
	else
		if Duel.NegateActivation(ev) and re:GetHandler():IsRelateToEffect(re) then
			Duel.Destroy(eg,REASON_EFFECT)
		end
	end
end

-- /// Effect 2: Position Change ///
function s.posfilter(c)
	return c:IsFaceup()
end
function s.postg(e,tp,eg,ep,ev,re,r,rp,chk)
	if chk==0 then return Duel.IsExistingMatchingCard(s.posfilter,tp,0,LOCATION_MZONE,1,nil) end
	local g=Duel.GetMatchingGroup(s.posfilter,tp,0,LOCATION_MZONE,nil)
	Duel.SetOperationInfo(0,CATEGORY_POSITION,g,#g,0,0)
end
function s.posop(e,tp,eg,ep,ev,re,r,rp)
	local g=Duel.GetMatchingGroup(s.posfilter,tp,0,LOCATION_MZONE,nil)
	if #g>0 then
		Duel.ChangePosition(g,POS_FACEDOWN_DEFENSE,POS_FACEUP_DEFENSE,POS_FACEUP_ATTACK,POS_FACEUP_ATTACK)
	end
end

-- /// Effect 3: Special Summon Banished Machines ///
function s.spcon(e,tp,eg,ep,ev,re,r,rp)
	local c=e:GetHandler()
	return c:IsPreviousPosition(POS_FACEUP) and c:GetPreviousControler()==tp
		and c:IsReason(REASON_EFFECT) and rp==1-tp
end
function s.spfilter(c,e,tp)
	return c:IsRace(RACE_MACHINE) and c:IsCanBeSpecialSummoned(e,0,tp,false,false)
end
function s.sptg(e,tp,eg,ep,ev,re,r,rp,chk)
	local rg=Duel.GetMatchingGroup(s.spfilter,tp,LOCATION_REMOVED,0,nil,e,tp)
	if chk==0 then return #rg>0 and Duel.GetLocationCount(tp,LOCATION_MZONE)>0 end
	Duel.SetOperationInfo(0,CATEGORY_SPECIAL_SUMMON,nil,1,tp,LOCATION_REMOVED)
end
function s.spop(e,tp,eg,ep,ev,re,r,rp)
	local ft=Duel.GetLocationCount(tp,LOCATION_MZONE)
	if ft<=0 then return end
	local rg=Duel.GetMatchingGroup(aux.NecroValleyFilter(s.spfilter),tp,LOCATION_REMOVED,0,nil,e,tp)
	if #rg==0 then return end
	
	-- Select any number of banished Machine monsters with different names
	Duel.Hint(HINT_SELECTMSG,tp,HINTMSG_SPSUMMON)
	local g=aux.SelectUnselectGroup(rg,e,tp,1,ft,aux.dncheck,1,tp,HINTMSG_SPSUMMON)
	if #g>0 then
		Duel.SpecialSummon(g,0,tp,tp,false,false,POS_FACEUP)
	end
end