-- Divine Golem of Ra
local s,id=GetID()
function s.initial_effect(c)
	-- Fusion Material / Special Summon Procedure
	c:EnableReviveLimit()
	Fusion.AddProcMix(c,true,true,s.matfilter1,s.matfilter2)
	
	-- Must first be Special Summoned by banishing the above cards from either side of the field
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

	-- Effect 1: Unaffected by other cards' effects, except Divine-Beast
	local e1=Effect.CreateEffect(c)
	e1:SetType(EFFECT_TYPE_SINGLE)
	e1:SetProperty(EFFECT_FLAG_SINGLE_RANGE)
	e1:SetRange(LOCATION_MZONE)
	e1:SetCode(EFFECT_IMMUNE_EFFECT)
	e1:SetValue(s.efilter)
	c:RegisterEffect(e1)

	-- Effect 2: Quick Effect - Tribute opponent's monsters and switch control
	local e2=Effect.CreateEffect(c)
	e2:SetDescription(aux.Stringid(id,0))
	e2:SetCategory(CATEGORY_RELEASE)
	e2:SetType(EFFECT_TYPE_QUICK_O)
	e2:SetCode(EVENT_FREE_CHAIN)
	e2:SetRange(LOCATION_MZONE)
	e2:SetCountLimit(1)
	e2:SetCondition(s.swcon)
	e2:SetTarget(s.swtg)
	e2:SetOperation(s.swop)
	c:RegisterEffect(e2)

	-- Effect 3: Return control during End Phase & take 2000 damage
	local e3=Effect.CreateEffect(c)
	e3:SetDescription(aux.Stringid(id,1))
	e3:SetCategory(CATEGORY_DAMAGE)
	e3:SetType(EFFECT_TYPE_FIELD+EFFECT_TYPE_TRIGGER_F)
	e3:SetCode(EVENT_PHASE+PHASE_END)
	e3:SetRange(LOCATION_MZONE)
	e3:SetCountLimit(1)
	e3:SetCondition(s.retcon)
	e3:SetOperation(s.retop)
	c:RegisterEffect(e3)
end

-- Official Card IDs
local CARD_WINGED_DRAGON_RA = 10000010 -- Standard playable/token ID variant
local CARD_LAVA_GOLEM        = 102380</code>