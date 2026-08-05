-- Black Luster Soldier, Envoy of the Eclipse
local s,id=GetID()
function s.initial_effect(c)
	c:EnableReviveLimit()
	-- 1 LIGHT Tuner + 1+ DARK Synchro non-Tuners
	Synchro.AddProcedure(c,aux.FilterBoolFunctionEx(Card.IsAttribute,ATTRIBUTE_LIGHT),1,99,s.ntfilter,1,1)

	-- Treat LIGHT "Black Luster Soldier" monsters as Tuners while in the Extra Deck
	local e0=Effect.CreateEffect(c)
	e0:SetType(EFFECT_TYPE_FIELD)
	e0:SetCode(EFFECT_ADD_TYPE)
	e0:SetRange(LOCATION_EXTRA)
	e0:SetTargetRange(LOCATION_MZONE,0)
	e0:SetTarget(s.tunertg)
	e0:SetValue(TYPE_TUNER)
	c:RegisterEffect(e0)

	-- Synchro Summon: Banish up to 2 cards
	local e1=Effect.CreateEffect(c)
	e1:SetCategory(CATEGORY_REMOVE)
	e1:SetType(EFFECT_TYPE_SINGLE+EFFECT_TYPE_TRIGGER_O)
	e1:SetCode(EVENT_SPSUMMON_SUCCESS)
	e1:SetProperty(EFFECT_FLAG_DELAY)
	e1:SetCondition(s.rmcon)
	e1:SetTarget(s.rmtg)
	e1:SetOperation(s.rmop)
	c:RegisterEffect(e1)

	-- Cannot be destroyed by battle
	local e2=Effect.CreateEffect(c)
	e2:SetType(EFFECT_TYPE_SINGLE)
	e2:SetCode(EFFECT_INDESTRUCTABLE_BATTLE)
	e2:SetValue(1)
	c:RegisterEffect(e2)

	-- Immune to opponent's effects if a card is banished this turn
	local e3=Effect.CreateEffect(c)
	e3:SetType(EFFECT_TYPE_SINGLE)
	e3:SetCode(EFFECT_IMMUNE_EFFECT)
	e3:SetProperty(EFFECT_FLAG_SINGLE_RANGE)
	e3:SetRange(LOCATION_MZONE)
	e3:SetCondition(s.immcon)
	e3:SetValue(s.efilter)
	c:RegisterEffect(e3)

	-- Global Tracker for banishments
	if not s.global_check then
		s.global_check=true
		local ge1=Effect.CreateEffect(c)
		ge1:SetType(EFFECT_TYPE_FIELD+EFFECT_TYPE_CONTINUOUS)
		ge1:SetCode(EVENT_REMOVE)
		ge1:SetOperation(s.checkop)
		Duel.RegisterEffect(ge1,0)
	end
	-- Reapply UI Tracker Hint
	local e4=Effect.CreateEffect(c)
	e4:SetType(EFFECT_TYPE_SINGLE+EFFECT_TYPE_CONTINUOUS)
	e4:SetCode(EVENT_SPSUMMON_SUCCESS)
	e4:SetOperation(s.hintop)
	c:RegisterEffect(e4)

	-- Passive Banish: Any monster destroyed by battle is banished
	local e5=Effect.CreateEffect(c)
	e5:SetType(EFFECT_TYPE_SINGLE)
	e5:SetCode(EFFECT_BATTLE_DESTROY_REDIRECT)
	e5:SetValue(LOCATION_REMOVED)
	c:RegisterEffect(e5)

	-- Battle Destruction Menu Follow-up
	local e6=Effect.CreateEffect(c)
	e6:SetDescription(aux.Stringid(id,3))
	e6:SetType(EFFECT_TYPE_SINGLE+EFFECT_TYPE_TRIGGER_O)
	e6:SetCode(EVENT_BATTLE_DESTROYING)
	e6:SetCondition(aux.bdocon)
	e6:SetTarget(s.menutg)
	e6:SetOperation(s.menuop)
	c:RegisterEffect(e6)
end

-- /// Filters for Synchro Summon ///
function s.ntfilter(c,sc,tp)
	return c:IsAttribute(ATTRIBUTE_DARK) and c:IsType(TYPE_SYNCHRO)
end
function s.tunertg(e,c)
	return c:IsAttribute(ATTRIBUTE_LIGHT) and c:IsSetCard(0x10cf)
end

-- /// Effect 1: Synchro Summon Banish ///
function s.rmcon(e,tp,eg,ep,ev,re,r,rp)
	return e:GetHandler():IsSummonType(SUMMON_TYPE_SYNCHRO)
end
function s.rmtg(e,tp,eg,ep,ev,re,r,rp,chk)
	if chk==0 then return Duel.IsExistingMatchingCard(Card.IsAbleToRemove,tp,0,LOCATION_HAND+LOCATION_ONFIELD+LOCATION_GRAVE,1,nil) end
	Duel.SetOperationInfo(0,CATEGORY_REMOVE,nil,1,1-tp,LOCATION_HAND+LOCATION_ONFIELD+LOCATION_GRAVE)
end
function s.rmop(e,tp,eg,ep,ev,re,r,rp)
	local g=Duel.GetMatchingGroup(Card.IsAbleToRemove,tp,0,LOCATION_HAND+LOCATION_ONFIELD+LOCATION_GRAVE,nil)
	if #g>0 then
		Duel.Hint(HINT_SELECTMSG,tp,HINTMSG_REMOVE)
		local sg=g:Select(tp,1,2,nil)
		if #sg>0 then
			Duel.Remove(sg,POS_FACEUP,REASON_EFFECT)
		end
	end
end

-- /// Global Banished Tracker & UI Hint ///
function s.checkop(e,tp,eg,ep,ev,re,r,rp)
	if Duel.GetFlagEffect(0,id)==0 then
		Duel.RegisterFlagEffect(0,id,RESET_PHASE+PHASE_END,0,1)
		local g=Duel.GetMatchingGroup(aux.FaceupFilter(Card.IsCode,id),0,LOCATION_MZONE,LOCATION_MZONE,nil)
		for tc in aux.Next(g) do
			tc:RegisterFlagEffect(id,RESET_EVENT+RESETS_STANDARD+RESET_PHASE+PHASE_END,EFFECT_FLAG_CLIENT_HINT,1,0,3110)
		end
	end
end
function s.hintop(e,tp,eg,ep,ev,re,r,rp)
	if Duel.GetFlagEffect(0,id)>0 then
		e:GetHandler():RegisterFlagEffect(id,RESET_EVENT+RESETS_STANDARD+RESET_PHASE+PHASE_END,EFFECT_FLAG_CLIENT_HINT,1,0,3110)
	end
end
function s.immcon(e)
	return Duel.GetFlagEffect(0,id)>0
end
function s.efilter(e,te)
	return te:GetOwnerPlayer()~=e:GetHandlerPlayer()
end

-- /// Battle Destruction Menu ///
function s.fdfilter(c,tp)
	return c:IsAbleToRemove(tp,POS_FACEDOWN)
end
function s.menutg(e,tp,eg,ep,ev,re,r,rp,chk)
	-- By stripping the "bc" check entirely from the chk==0 block, the engine doesn't care 
	-- that the destroyed monster has already been physically moved to the banishment zone.
	if chk==0 then
		local b1=Duel.GetTurnPlayer()==tp
		local b2=Duel.IsExistingMatchingCard(s.fdfilter,tp,LOCATION_ONFIELD,LOCATION_ONFIELD,1,nil,tp)
		local b3=true -- ATK gain is always a valid choice
		return b1 or b2 or b3
	end
	
	local b1=Duel.GetTurnPlayer()==tp
	local b2=Duel.IsExistingMatchingCard(s.fdfilter,tp,LOCATION_ONFIELD,LOCATION_ONFIELD,1,nil,tp)
	local b3=true
	
	local op=Duel.SelectEffect(tp,
		{b1, aux.Stringid(id,0)}, 
		{b2, aux.Stringid(id,1)}, 
		{b3, aux.Stringid(id,2)}  
	)
	
	e:SetLabel(op)
	if op==2 then
		e:SetCategory(CATEGORY_REMOVE)
		local g=Duel.GetMatchingGroup(s.fdfilter,tp,LOCATION_ONFIELD,LOCATION_ONFIELD,nil,tp)
		Duel.SetOperationInfo(0,CATEGORY_REMOVE,g,1,tp,POS_FACEDOWN)
	elseif op==3 then
		e:SetCategory(CATEGORY_ATKCHANGE)
	end
end
function s.menuop(e,tp,eg,ep,ev,re,r,rp)
	local c=e:GetHandler()
	local op=e:GetLabel()
	
	if not c:IsRelateToEffect(e) or c:IsFacedown() then return end
	
	if op==1 and Duel.GetTurnPlayer()==tp then
		-- Register a flag to increment attacks safely
		c:RegisterFlagEffect(id+1,RESET_EVENT+RESETS_STANDARD+RESET_PHASE+PHASE_END,0,1)
		local e1=Effect.CreateEffect(c)
		e1:SetType(EFFECT_TYPE_SINGLE)
		e1:SetProperty(EFFECT_FLAG_CANNOT_DISABLE)
		e1:SetCode(EFFECT_EXTRA_ATTACK)
		e1:SetValue(c:GetFlagEffect(id+1))
		e1:SetReset(RESET_EVENT+RESETS_STANDARD+RESET_PHASE+PHASE_END)
		c:RegisterEffect(e1)
		
	elseif op==2 then
		Duel.Hint(HINT_SELECTMSG,tp,HINTMSG_REMOVE)
		local g=Duel.SelectMatchingCard(tp,s.fdfilter,tp,LOCATION_ONFIELD,LOCATION_ONFIELD,1,1,nil,tp)
		if #g>0 then
			Duel.Remove(g,POS_FACEDOWN,REASON_EFFECT)
		end
		
	elseif op==3 then
		local e1=Effect.CreateEffect(c)
		e1:SetType(EFFECT_TYPE_SINGLE)
		e1:SetCode(EFFECT_UPDATE_ATTACK)
		e1:SetValue(1500)
		e1:SetReset(RESET_EVENT+RESETS_STANDARD+RESET_DISABLE)
		c:RegisterEffect(e1)
	end
end