-- Divine Magician of Slifer
local s,id=GetID()
function s.initial_effect(c)
	-- Fusion Material 
	c:EnableReviveLimit()
	Fusion.AddProcMix(c,true,true,s.matfilter1,s.matfilter2)
	
	-- Must first be Fusion Summoned by sending the above cards to the GY
	local e0=Effect.CreateEffect(c)
	e0:SetType(EFFECT_TYPE_SINGLE)
	e0:SetProperty(EFFECT_FLAG_CANNOT_DISABLE+EFFECT_FLAG_UNCOPYABLE)
	e0:SetCode(EFFECT_SPSUMMON_CONDITION)
	e0:SetValue(aux.fuslimit)
	c:RegisterEffect(e0)

	-- Contact Fusion Procedure
	local e0a=Effect.CreateEffect(c)
	e0a:SetType(EFFECT_TYPE_FIELD)
	e0a:SetProperty(EFFECT_FLAG_UNCOPYABLE)
	e0a:SetCode(EFFECT_SPSUMMON_PROC)
	e0a:SetRange(LOCATION_EXTRA)
	e0a:SetCondition(s.contactcon)
	e0a:SetTarget(s.contacttg)
	e0a:SetOperation(s.contactop)
	e0a:SetValue(SUMMON_TYPE_FUSION)
	c:RegisterEffect(e0a)

	-- Effect 1: Unaffected by other cards' effects, except Divine-Beast
	local e1=Effect.CreateEffect(c)
	e1:SetType(EFFECT_TYPE_SINGLE)
	e1:SetProperty(EFFECT_FLAG_SINGLE_RANGE)
	e1:SetRange(LOCATION_MZONE)
	e1:SetCode(EFFECT_IMMUNE_EFFECT)
	e1:SetValue(s.efilter)
	c:RegisterEffect(e1)

	-- Effect 2: Gains 1000 ATK/DEF for each card in your hand
	local e2=Effect.CreateEffect(c)
	e2:SetType(EFFECT_TYPE_SINGLE)
	e2:SetProperty(EFFECT_FLAG_SINGLE_RANGE)
	e2:SetRange(LOCATION_MZONE)
	e2:SetCode(EFFECT_UPDATE_ATTACK)
	e2:SetValue(s.val)
	c:RegisterEffect(e2)
	local e3=e2:Clone()
	e3:SetCode(EFFECT_UPDATE_DEFENSE)
	c:RegisterEffect(e3)

	-- Effect 3: Decrease opponent's summoned monster ATK based on their hand size & destroy if 0
	local e4=Effect.CreateEffect(c)
	e4:SetDescription(aux.Stringid(id,0))
	e4:SetCategory(CATEGORY_ATKCHANGE+CATEGORY_DESTROY)
	e4:SetType(EFFECT_TYPE_FIELD+EFFECT_TYPE_TRIGGER_F)
	e4:SetRange(LOCATION_MZONE)
	e4:SetCode(EVENT_SUMMON_SUCCESS)
	e4:SetCondition(s.atkcon)
	e4:SetTarget(s.atktg)
	e4:SetOperation(s.atkop)
	c:RegisterEffect(e4)
	local e5=e4:Clone()
	e5:SetCode(EVENT_SPSUMMON_SUCCESS)
	c:RegisterEffect(e5)

	-- Effect 4: Target 1 card in your GY; add it to your hand
	local e6=Effect.CreateEffect(c)
	e6:SetDescription(aux.Stringid(id,1))
	e6:SetCategory(CATEGORY_TOHAND)
	e6:SetType(EFFECT_TYPE_IGNITION)
	e6:SetRange(LOCATION_MZONE)
	e6:SetCountLimit(1)
	e6:SetProperty(EFFECT_FLAG_CARD_TARGET)
	e6:SetTarget(s.thtg)
	e6:SetOperation(s.thop)
	c:RegisterEffect(e6)
end

-- Official Card IDs
local CARD_SLIFER_SKY_DRAGON = 10000020
local CARD_DARK_MAGICIAN     = 46986414

-- Fusion Material Filters
function s.matfilter1(c,fc,sumtype,tp)
	return c:IsCode(CARD_SLIFER_SKY_DRAGON)
end
function s.matfilter2(c,fc,sumtype,tp)
	return c:IsCode(CARD_DARK_MAGICIAN)
end

-- /// Send to GY Contact Procedure ///
function s.contactfilter(c)
	return c:IsFaceup() and c:IsAbleToGraveAsCost() and (c:IsCode(CARD_SLIFER_SKY_DRAGON) or c:IsCode(CARD_DARK_MAGICIAN))
end
function s.rescon(sg,e,tp,mg)
	return Duel.GetLocationCountFromEx(tp,tp,sg,e:GetHandler())>0 
		and sg:IsExists(Card.IsCode,1,nil,CARD_SLIFER_SKY_DRAGON) 
		and sg:IsExists(Card.IsCode,1,nil,CARD_DARK_MAGICIAN)
end
function s.contactcon(e,c)
	if c==nil then return true end
	local tp=c:GetControler()
	local g=Duel.GetMatchingGroup(s.contactfilter,tp,LOCATION_MZONE,0,nil)
	return aux.SelectUnselectGroup(g,e,tp,2,2,s.rescon,0)
end
function s.contacttg(e,tp,eg,ep,ev,re,r,rp,c)
	local g=Duel.GetMatchingGroup(s.contactfilter,tp,LOCATION_MZONE,0,nil)
	local sg=aux.SelectUnselectGroup(g,e,tp,2,2,s.rescon,1,tp,HINTMSG_TOGRAVE,nil,nil,true)
	if #sg>0 then
		sg:KeepAlive()
		e:SetLabelObject(sg)
		return true
	end
	return false
end
function s.contactop(e,tp,eg,ep,ev,re,r,rp,c)
	local sg=e:GetLabelObject()
	if sg then
		Duel.SendtoGrave(sg,REASON_COST+REASON_MATERIAL)
		sg:DeleteGroup()
	end
end

-- /// Effect 1: Immunity Filter ///
function s.efilter(e,te)
	return te:GetOwner()~=e:GetOwner() and not te:GetHandler():IsRace(RACE_DIVINE)
end

-- /// Effect 2: Stat Boost Calculation ///
function s.val(e,c)
	return Duel.GetFieldGroupCount(e:GetHandlerPlayer(),LOCATION_HAND,0)*1000
end

-- /// Effect 3: Opponent Summon ATK Reduction & Destruction ///
function s.atkfilter(c,tp)
	-- Unlike official Slifer which specifies Attack position, this applies face-up generally
	return c:IsControler(1-tp) and c:IsFaceup()
end
function s.atkcon(e,tp,eg,ep,ev,re,r,rp)
	return not eg:IsContains(e:GetHandler()) and eg:IsExists(s.atkfilter,1,nil,tp)
end
function s.atktg(e,tp,eg,ep,ev,re,r,rp,chk)
	if chk==0 then return e:GetHandler():IsRelateToEffect(e) end
	Duel.SetTargetCard(eg:Filter(s.atkfilter,nil,tp))
end
function s.atkop(e,tp,eg,ep,ev,re,r,rp)
	local g=Duel.GetTargetCards(e):Filter(Card.IsFaceup,nil)
	if #g==0 then return end
	
	-- Calculate ATK loss dynamically based on opponent's hand size
	local val=Duel.GetFieldGroupCount(1-tp,LOCATION_HAND,0)*1000
	if val==0 then return end
	
	local dg=Group.CreateGroup()
	local c=e:GetHandler()
	for tc in aux.Next(g) do
		local preatk=tc:GetAttack()
		local e1=Effect.CreateEffect(c)
		e1:SetType(EFFECT_TYPE_SINGLE)
		e1:SetCode(EFFECT_UPDATE_ATTACK)
		e1:SetValue(-val)
		e1:SetReset(RESET_EVENT+RESETS_STANDARD)
		tc:RegisterEffect(e1)
		
		-- Destroy if ATK dropped to exactly 0
		if preatk~=0 and tc:GetAttack()==0 then 
			dg:AddCard(tc) 
		end
	end
	if #dg==0 then return end
	Duel.BreakEffect()
	Duel.Destroy(dg,REASON_EFFECT)
end

-- /// Effect 4: Target 1 card in your GY; add to hand ///
function s.thfilter(c)
	return c:IsAbleToHand()
end
function s.thtg(e,tp,eg,ep,ev,re,r,rp,chk,chkc)
	if chkc then return chkc:IsLocation(LOCATION_GRAVE) and chkc:IsControler(tp) and s.thfilter(chkc) end
	if chk==0 then return Duel.IsExistingTarget(s.thfilter,tp,LOCATION_GRAVE,0,1,nil) end
	Duel.Hint(HINT_SELECTMSG,tp,HINTMSG_ATOHAND)
	local g=Duel.SelectTarget(tp,s.thfilter,tp,LOCATION_GRAVE,0,1,1,nil)
	Duel.SetOperationInfo(0,CATEGORY_TOHAND,g,1,0,0)
end
function s.thop(e,tp,eg,ep,ev,re,r,rp)
	local tc=Duel.GetFirstTarget()
	if tc and tc:IsRelateToEffect(e) then
		Duel.SendtoHand(tc,nil,REASON_EFFECT)
	end
end